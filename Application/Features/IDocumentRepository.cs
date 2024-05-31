using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Shared;

namespace Seatpicker.Application.Features;

public interface IDocumentRepository
{
    public IDocumentTransaction CreateTransaction(string? guildId = null);

    public IDocumentReader CreateReader(string? guildId = null);
}

public interface IDocumentReader : IDisposable
{
    public Task<TDocument?> Query<TDocument>(string id)
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

    public void DeleteWhere<TDocument>(Expression<Func<TDocument, bool>> where)
        where TDocument : IDocument;

    public Task Commit();
}