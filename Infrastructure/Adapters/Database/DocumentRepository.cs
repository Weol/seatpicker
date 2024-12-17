using System.Linq.Expressions;
using Marten;
using Marten.Storage;
using Seatpicker.Application.Features;
using Shared;

namespace Seatpicker.Infrastructure.Adapters.Database;

public class DocumentRepository(IServiceProvider provider, ILogger<DocumentRepository> logger)
{
    public IDocumentTransaction CreateTransaction(string guildId)
    {
        var documentSession = provider.GetRequiredService<IDocumentStore>().LightweightSession(guildId);
        
        if (documentSession.TenantId == Tenancy.DefaultTenantId)
        {
            throw new InvalidTenantIdForGuildSessionException {
                TenantId = documentSession.TenantId,
            };
        }

        var transactionId = Interlocked.Add(ref DatabaseExtensions.SessionIdCounter, 1);
        logger.LogDebug("[{TransactionId}] Creating document transaction for tenant {TenantId}", transactionId, documentSession.TenantId);

        var transactionLogger = provider.GetRequiredService<ILogger<DocumentTransaction>>();
        return new DocumentTransaction(documentSession, transactionId, transactionLogger);
    }

    public virtual IDocumentReader CreateReader(string guildId)
    {
        var querySession = provider.GetRequiredService<IDocumentStore>().QuerySession(guildId);
        
        if (querySession.TenantId == Tenancy.DefaultTenantId)
        {
            throw new InvalidTenantIdForGuildSessionException {
                TenantId = querySession.TenantId,
            };
        }

        var readerId = Interlocked.Add(ref DatabaseExtensions.SessionIdCounter, 1);
        logger.LogDebug("[{ReaderId}] Creating document reader for tenant {TenantId}", readerId, querySession.TenantId);

        var readerLogger = provider.GetRequiredService<ILogger<DocumentReader>>();
        return new DocumentReader(querySession, readerId, readerLogger);
    }

    public virtual IGuildlessDocumentTransaction CreateGuildlessTransaction()
    {
        var documentSession = provider.GetRequiredService<IDocumentStore>().LightweightSession();

        if (documentSession.TenantId != Tenancy.DefaultTenantId)
        {
            throw new InvalidTenantIdForGuildlessSessionException {
                TenantId = documentSession.TenantId,
            };
        }

        var transactionId = Interlocked.Add(ref DatabaseExtensions.SessionIdCounter, 1);
        logger.LogDebug("[{ReaderId}] Creating document transaction for tenant {TenantId}", transactionId, documentSession.TenantId);

        var transactionLogger = provider.GetRequiredService<ILogger<DocumentTransaction>>();
        return new DocumentTransaction(documentSession, transactionId, transactionLogger);
    }

    public IGuildlessDocumentReader CreateGuildlessReader()
    {
        var querySession = provider.GetRequiredService<IDocumentStore>().QuerySession();

        if (querySession.TenantId != Tenancy.DefaultTenantId)
        {
            throw new InvalidTenantIdForGuildlessSessionException {
                TenantId = querySession.TenantId,
            };
        }

        var readerId = Interlocked.Add(ref DatabaseExtensions.SessionIdCounter, 1);
        logger.LogDebug("[{ReaderId}] Creating document reader for tenant {TenantId}", readerId, querySession.TenantId);

        var readerLogger = provider.GetRequiredService<ILogger<DocumentReader>>();
        return new DocumentReader(querySession, readerId, readerLogger);
    }

    public class InvalidTenantIdForGuildlessSessionException : Exception
    {
        public required string TenantId { get; init; }
    };
    
    public class InvalidTenantIdForGuildSessionException : Exception
    {
        public required string TenantId { get; init; }
    };
}

public class DocumentTransaction(IDocumentSession session, int transactionId, ILogger<DocumentTransaction> logger) : IGuildlessDocumentTransaction
{
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
        logger.LogDebug("[{TransactionId}] Disposing transaction for tenant {TenantId}", transactionId, session.TenantId);
        return session.SaveChangesAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await session.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}

public class DocumentReader(IQuerySession session, int readerId, ILogger<DocumentReader> logger) : IGuildlessDocumentReader
{
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

    public async ValueTask DisposeAsync()
    {
        logger.LogDebug("[{ReaderId}] Disposing document reader for tenant {TenantId}", readerId, session.TenantId);
        await session.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}