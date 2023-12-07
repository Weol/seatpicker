using Marten;
using Seatpicker.Application.Features;
using Shared;

namespace Seatpicker.Infrastructure.Adapters.Database;

public class DocumentRepository : IDocumentRepository
{
    private readonly IDocumentStore store;

    public DocumentRepository(IDocumentStore store, ILogger<DocumentRepository> logger)
    {
        this.store = store;
    }

    public IDocumentTransaction CreateTransaction()
    {
        var session = store.LightweightSession();
        return new DocumentTransaction(session);
    }

    public IDocumentReader CreateReader()
    {
        var session = store.QuerySession();
        return new DocumentReader(session);
    }
}

public class DocumentTransaction : IDocumentTransaction
{
    private readonly IDocumentSession session;
    private readonly DocumentReader reader;

    public DocumentTransaction(IDocumentSession session)
    {
        this.session = session;
        reader = new DocumentReader(session);
    }

    public void Store<TDocument>(params TDocument[] documentsToAdd)
        where TDocument : IDocument
    {
        session.Store(documentsToAdd);
    }

    public void Delete<TDocument>(string id)
        where TDocument : IDocument
    {
        session.Delete<TDocument>(id);
    }

    public Task Commit()
    {
        return session.SaveChangesAsync();
    }

    public Task<TDocument?> Get<TDocument>(string id)
        where TDocument : IDocument =>
        reader.Get<TDocument>(id);

    public Task<bool> Exists<TDocument>(string id)
        where TDocument : IDocument =>
        reader.Exists<TDocument>(id);

    public IQueryable<TDocument> Query<TDocument>()
        where TDocument : IDocument =>
        reader.Query<TDocument>();

    public void Dispose()
    {
        session.Dispose();
        reader.Dispose();
    }
}

public class DocumentReader : IDocumentReader
{
    private readonly IQuerySession session;

    public DocumentReader(IQuerySession session)
    {
        this.session = session;
    }

    public Task<TDocument?> Get<TDocument>(string id)
        where TDocument : IDocument
    {
        return session.LoadAsync<TDocument>(id);
    }

    public async Task<bool> Exists<TDocument>(string id)
        where TDocument : IDocument
    {
        return await Get<TDocument>(id) is not null;
    }

    public IQueryable<TDocument> Query<TDocument>()
        where TDocument : IDocument
    {
        return session.Query<TDocument>();
    }

    public void Dispose()
    {
        session.Dispose();
    }
}