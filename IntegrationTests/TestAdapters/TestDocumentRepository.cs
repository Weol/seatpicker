using Marten;
using NSubstitute;
using Seatpicker.Application.Features;
using Shared;

namespace Seatpicker.IntegrationTests.TestAdapters;

#pragma warning disable CS1998 //Async method lacks await

public class TestDocumentRepository : IDocumentRepository
{
    public IDictionary<string, IDocument> Documents { get; } = new Dictionary<string, IDocument>();

    public IDocumentTransaction CreateTransaction()
    {
        return new TestDocumentTransaction(Documents);
    }

    public IDocumentReader CreateReader()
    {
        return new TestDocumentReader(Documents);
    }
}

public class TestDocumentTransaction : IDocumentTransaction
{
    private readonly IDictionary<string, IDocument> documents;

    private readonly TestDocumentReader reader;
    private readonly IList<IDocument> stagedDocuments = new List<IDocument>();
    private readonly IList<string> stagedDeletions = new List<string>();

    public TestDocumentTransaction(IDictionary<string, IDocument> documents)
    {
        this.documents = documents;
        reader = new TestDocumentReader(documents);
    }

    public void Store<TDocument>(params TDocument[] documentsToAdd)
        where TDocument : IDocument
    {
        foreach (var document in documentsToAdd)
        {
            if (documents.ContainsKey(document.Id)) throw new DocumentAlreadyExistsException
                {
                    Id = document.Id,
                    Type = document.GetType(),
                };

            stagedDocuments.Add(document);
        }
    }

    public void Delete<TDocument>(string id)
        where TDocument : IDocument
    {
        if (!documents.ContainsKey(id)) throw new DocumentDoesNotExistException
            {
                Id = id,
                Type = typeof(TDocument),
            };

        stagedDeletions.Add(id);
    }

    public void Commit()
    {
        foreach (var document in stagedDocuments)
        {
            documents[document.Id] = document;
        }

        foreach (var documentId in stagedDeletions)
        {
            documents.Remove(documentId);
        }
    }

    public Task<TDocument?> Get<TDocument>(string id)
        where TDocument : IDocument =>
        reader.Get<TDocument>(id);

    public IQueryable<TDocument> Query<TDocument>()
        where TDocument : IDocument =>
        reader.Query<TDocument>();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public void Dispose()
    {
    }
}

public class TestDocumentReader : IDocumentReader
{
    private readonly IDictionary<string, IDocument> documents;

    public TestDocumentReader(IDictionary<string, IDocument> documents)
    {
        this.documents = documents;
    }

    public async Task<TDocument?> Get<TDocument>(string id)
        where TDocument : IDocument
    {
        if (!documents.TryGetValue(id, out var document)) return default;

        if (document is not TDocument typedDocument) throw new DocumentTypeMismatchException
        {
            Id = id,
            ExpectedType = typeof(TDocument),
            ActualType = document .GetType(),
        };

        return typedDocument;
    }

    public IQueryable<TDocument> Query<TDocument>()
        where TDocument : IDocument
    {
        return documents.Values
            .OfType<TDocument>()
            .AsQueryable();
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}

public class DocumentTypeMismatchException : Exception
{
    public required string Id { get; init; }
    public required Type ExpectedType { get; init; }
    public required Type ActualType { get; init; }

    public override string Message =>
        $"Type mismatch in test store, expected aggregate with id {Id} to be of type {ExpectedType.Name} but was actually {ActualType.Name}";
}

public class DocumentAlreadyExistsException : Exception
{
    public required string Id { get; init; }
    public required Type Type { get; init; }

    public override string Message =>
        $"Document of type {Type.Name} and id {Id} already exists in test store";
}

public class DocumentDoesNotExistException : Exception
{
    public required string Id { get; init; }
    public required Type Type { get; init; }

    public override string Message =>
        $"Document of type {Type.Name} and id {Id} dose not exist in test store";
}
