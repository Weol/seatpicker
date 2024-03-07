using System.Linq.Expressions;
using Marten;
using Marten.Storage;
using Seatpicker.Application.Features;
using Shared;

namespace Seatpicker.Infrastructure.Adapters.Database;

public class DocumentRepository : IDocumentRepository
{
    private readonly IDocumentStore store;
    private readonly ILogger<DocumentRepository> logger;
    private readonly GuildIdProvider guildIdProvider;

    public DocumentRepository(IDocumentStore store, GuildIdProvider guildIdProvider, ILogger<DocumentRepository> logger)
    {
        this.store = store;
        this.guildIdProvider = guildIdProvider;
        this.logger = logger;
    }

    public IDocumentTransaction CreateTransaction(string? guildId = null)
    {
        guildId ??= guildIdProvider.GetGuildId();

        logger.LogInformation("Creating document transaction for tenant {TenantId}", guildId);

        var session = store.LightweightSession(guildId);
        return new DocumentTransaction(session);
    }

    public IDocumentReader CreateReader(string? guildId = null)
    {
        guildId ??= guildIdProvider.GetGuildId();

        logger.LogInformation("Creating document reader for tenant {TenantId}", guildId);

        var session = store.QuerySession(guildId);
        return new DocumentReader(session);
    }
}

public class GlobalDocumentRepository
{
    private const string DefaultTenant = "*DEFAULT*";
    
    private readonly IDocumentStore store;
    private readonly ILogger<DocumentRepository> logger;

    public GlobalDocumentRepository(IDocumentStore store, ILogger<DocumentRepository> logger)
    {
        this.store = store;
        this.logger = logger;
    }

    public IDocumentTransaction CreateTransaction()
    {
        logger.LogInformation("Creating document transaction for default tenant");

        var session = store.LightweightSession(Tenancy.DefaultTenantId);
        return new DocumentTransaction(session);
    }

    public IDocumentReader CreateReader()
    {
        logger.LogInformation("Creating document reader for default tenant");

        var session = store.QuerySession(Tenancy.DefaultTenantId);
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
    
    public void DeleteWhere<TDocument>(Expression<Func<TDocument,bool>> where)
        where TDocument : IDocument
    {
        session.DeleteWhere(where);
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