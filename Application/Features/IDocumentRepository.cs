using Seatpicker.Domain;
using Shared;

namespace Seatpicker.Application.Features;

public interface IDocumentRepository
{
    public IDocumentTransaction CreateTransaction();

    public IDocumentReader CreateReader();
}

public interface IDocumentReader : IAsyncDisposable
{
    public Task<TDocument?> Get<TDocument>(string id)
        where TDocument : IDocument;

    public IQueryable<TDocument> Query<TDocument>()
        where TDocument : IDocument;
}

public interface IDocumentTransaction : IDocumentReader, IAsyncDisposable, IDisposable
{
    public void Store<TDocument>(params TDocument[] documentsToAdd)
        where TDocument : IDocument;

    public void Delete<TDocument>(string id)
        where TDocument : IDocument;

    public void Commit();
}