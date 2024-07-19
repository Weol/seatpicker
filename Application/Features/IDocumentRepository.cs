using System.Linq.Expressions;
using Marten;
using Shared;

namespace Seatpicker.Application.Features;

public interface IDocumentRepository
{
    public IDocumentTransaction CreateTransaction(string guildId, IDocumentSession documentSession);

    public IDocumentReader CreateReader(string guildId, IQuerySession querySession);

    public IGuildlessDocumentTransaction CreateGuildlessTransaction(IDocumentSession documentSession);

    public IGuildlessDocumentReader CreateGuildlessReader(IQuerySession querySession);
}

public interface IGuildlessDocumentTransaction : IDocumentTransaction;

public interface IGuildlessDocumentReader : IDocumentReader;

public interface IDocumentReader : IDisposable
{
    public Task<TDocument?> Query<TDocument>(string id)
        where TDocument : IDocument;

    public IQueryable<TDocument> Query<TDocument>()
        where TDocument : IDocument;
}

public interface IDocumentTransaction : IDisposable
{
    public void Store<TDocument>(params TDocument[] documentsToAdd)
        where TDocument : IDocument;

    public void Delete<TDocument>(string id)
        where TDocument : IDocument;

    public void DeleteWhere<TDocument>(Expression<Func<TDocument, bool>> where)
        where TDocument : IDocument;

    public Task Commit();
}