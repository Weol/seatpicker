using System.Linq.Expressions;
using Marten;
using Marten.Storage;
using Seatpicker.Application.Features;
using Shared;

namespace Seatpicker.Infrastructure.Adapters.Database;

public class DocumentRepository(IDocumentStore store, ILogger<DocumentRepository> logger) : IDocumentRepository
{
    public IDocumentTransaction CreateTransaction(string guildId, IDocumentSession? documentSession = null)
    {
        logger.LogInformation("Creating document transaction for tenant {TenantId}", guildId);

        documentSession ??= store.LightweightSession(guildId);

        return new DocumentTransaction(documentSession);
    }

    public virtual IDocumentReader CreateReader(string guildId, IQuerySession? querySession = null)
    {
        logger.LogInformation("Creating document reader for tenant {TenantId}", guildId);

        querySession ??= store.QuerySession(guildId);

        return new DocumentReader(querySession);
    }

    public virtual IGuildlessDocumentTransaction CreateGuildlessTransaction(IDocumentSession? documentSession = null)
    {
        logger.LogInformation("Creating document transaction for default tenant");

        documentSession ??= store.LightweightSession(Tenancy.DefaultTenantId);

        if (documentSession.TenantId != Tenancy.DefaultTenantId)
        {
            throw new InvalidTenantIdForGuildlessSessionException {
                TenantId = documentSession.TenantId,
            };
        }

        return new DocumentTransaction(documentSession);
    }

    public IGuildlessDocumentReader CreateGuildlessReader(IQuerySession? querySession = null)
    {
        logger.LogInformation("Creating document reader for default tenant");

        querySession ??= store.QuerySession(Tenancy.DefaultTenantId);

        if (querySession.TenantId != Tenancy.DefaultTenantId)
        {
            throw new InvalidTenantIdForGuildlessSessionException {
                TenantId = querySession.TenantId,
            };
        }

        return new DocumentReader(querySession);
    }

    public class InvalidTenantIdForGuildlessSessionException : Exception
    {
        public string TenantId { get; init; }
    };
}

public class DocumentTransaction : IGuildlessDocumentTransaction, IDisposable
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

    public void DeleteWhere<TDocument>(Expression<Func<TDocument,bool>> where)
        where TDocument : IDocument
    {
        session.DeleteWhere(where);
    }

    public Task Commit()
    {
        return session.SaveChangesAsync();
    }

    public Task<TDocument?> Query<TDocument>(string id)
        where TDocument : IDocument =>
        reader.Query<TDocument>(id);

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
        GC.SuppressFinalize(this);
    }
}

public class DocumentReader : IGuildlessDocumentReader
{
    private readonly IQuerySession session;

    public DocumentReader(IQuerySession session)
    {
        this.session = session;
    }

    public Task<TDocument?> Query<TDocument>(string id)
        where TDocument : IDocument
    {
        return session.LoadAsync<TDocument>(id);
    }

    public async Task<bool> Exists<TDocument>(string id)
        where TDocument : IDocument
    {
        return await Query<TDocument>(id) is not null;
    }

    public IQueryable<TDocument> Query<TDocument>()
        where TDocument : IDocument
    {
        return session.Query<TDocument>();
    }

    public void Dispose()
    {
        session.Dispose();
        GC.SuppressFinalize(this);
    }
}