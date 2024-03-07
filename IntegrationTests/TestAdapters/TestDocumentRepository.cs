using System.Linq.Expressions;
using Marten;
using Marten.Storage;
using Microsoft.Extensions.Logging;
using Seatpicker.Application.Features;
using Seatpicker.Infrastructure.Adapters.Database;
using Shared;

namespace Seatpicker.IntegrationTests.TestAdapters;

public class TestDocumentRepository : IDocumentRepository
{
    private readonly DocumentRepository backingRepository;

    public TestDocumentRepository(IDocumentStore store, GuildIdProvider guildIdProvider, ILogger<DocumentRepository> logger)
    {
        backingRepository = new DocumentRepository(store, guildIdProvider, logger);
    }

    public IDocumentTransaction CreateTransaction(string? guildId = null)
    {
        return new TestDocumentTransaction(backingRepository.CreateTransaction(guildId), this);
    }

    public IDocumentReader CreateReader(string? guildId = null)
    {
        return backingRepository.CreateReader(guildId);
    }
    
    public IDocumentTransaction CreateGlobalTransaction()
    {
        return new TestDocumentTransaction(backingRepository.CreateGlobalTransaction(), this);
    }

    public IDocumentReader CreateGlobalReader()
    {
        return backingRepository.CreateGlobalReader();
    }
}

public class TestDocumentTransaction : IDocumentTransaction
{
    private readonly object lockObject;
    private readonly IDocumentTransaction backingTransaction;

    public TestDocumentTransaction(IDocumentTransaction transaction, object lockObject)
    {
        this.lockObject = lockObject;
        
        backingTransaction = transaction;
    }

    public Task Commit()
    {
        // On macOS the tests will deadlock if multiple tests run SaveChanges at the same time
        lock (lockObject)
        {
            backingTransaction.Commit()
                .GetAwaiter()
                .GetResult();
        }

        return Task.CompletedTask;
    }
    
    public void Store<TDocument>(params TDocument[] documentsToAdd) where TDocument : IDocument
    {
        backingTransaction.Store(documentsToAdd);
    }

    public void Delete<TDocument>(string id) where TDocument : IDocument
    {
        backingTransaction.Delete<TDocument>(id);
    }

    public void DeleteWhere<TDocument>(Expression<Func<TDocument, bool>> where) where TDocument : IDocument
    {
        backingTransaction.DeleteWhere(where);
    }

    public void Dispose()
    {
        backingTransaction.Dispose();
    }

    public Task<TDocument?> Get<TDocument>(string id) where TDocument : IDocument
    {
        return backingTransaction.Get<TDocument>(id);
    }

    public IQueryable<TDocument> Query<TDocument>() where TDocument : IDocument
    {
        return backingTransaction.Query<TDocument>();
    }
}
