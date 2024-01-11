using Shared;

namespace Seatpicker.Application.Features;

public interface IDocumentRepository
{
    public IDocumentTransaction CreateTransaction(string? tenant = null);

    public IDocumentReader CreateReader(string? tenant = null);
}

public interface IDocumentReader : IDisposable
{
    public Task<TDocument?> Get<TDocument>(string id)
        where TDocument : IDocument;

    public IQueryable<TDocument> Query<TDocument>()
        where TDocument : IDocument;
}

public interface IDocumentTransaction : IDocumentReader
{
    public void Store<TDocument>(params TDocument[] documentsToAdd)
        where TDocument : IDocument;

    public void Delete<TDocument>(string id)
        where TDocument : IDocument;

    public Task Commit();
}