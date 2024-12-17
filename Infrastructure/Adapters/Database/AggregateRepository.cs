using System.Diagnostics;
using System.Timers;
using Marten;
using Marten.Storage;
using Seatpicker.Application.Features;
using Shared;
using Timer = System.Timers.Timer;

namespace Seatpicker.Infrastructure.Adapters.Database;

public class AggregateRepository(ILogger<AggregateRepository> logger, IServiceProvider provider)
{
    public IAggregateTransaction CreateTransaction(string guildId)
    {
        var documentSession = provider.GetRequiredService<IDocumentStore>().LightweightSession(guildId);
        
        if (documentSession.TenantId == Tenancy.DefaultTenantId)
        {
            throw new InvalidTenantIdForGuildSessionException {
                TenantId = documentSession.TenantId,
            };
        }

        var transactionId = Interlocked.Add(ref DatabaseExtensions.SessionIdCounter, 1);
        logger.LogDebug("[{TransactionId}] Creating aggregate transaction for tenant {TenantId}", transactionId, documentSession.TenantId);

        var transactionLogger = provider.GetRequiredService<ILogger<AggregateTransaction>>();
        return new AggregateTransaction(documentSession, transactionId, transactionLogger);
    }

    public IGuildlessAggregateTransaction CreateGuildlessTransaction()
    {
        var documentSession = provider.GetRequiredService<IDocumentStore>().LightweightSession();
        
        if (documentSession.TenantId != Tenancy.DefaultTenantId)
        {
            throw new InvalidTenantIdForGuildlessSessionException {
                TenantId = documentSession.TenantId,
            };
        }

        var transactionId = Interlocked.Add(ref DatabaseExtensions.SessionIdCounter, 1);
        logger.LogDebug("[{TransactionId}] Creating aggregate transaction for tenant {TenantId}", transactionId, documentSession.TenantId);

        var transactionLogger = provider.GetRequiredService<ILogger<AggregateTransaction>>();
        return new AggregateTransaction(documentSession, transactionId, transactionLogger);
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

public class AggregateTransaction(IDocumentSession session, int transactionId, ILogger<AggregateTransaction> logger) : IGuildlessAggregateTransaction
{
    public void Update<TAggregate>(TAggregate aggregate)
        where TAggregate : AggregateBase
    {
        session.Events.Append(aggregate.Id, aggregate.RaisedEvents);
    }

    public void Create<TAggregate>(TAggregate aggregate)
        where TAggregate : AggregateBase
    {
        session.Events.StartStream<TAggregate>(aggregate.Id, aggregate.RaisedEvents);
    }

    public void Archive<TAggregate>(TAggregate aggregate)
        where TAggregate : AggregateBase
    {
        session.Events.ArchiveStream(aggregate.Id);
    }

    public Task Commit()
    {
        return session.SaveChangesAsync();
    }

    public Task<TAggregate?> Aggregate<TAggregate>(string id)
        where TAggregate : AggregateBase
    {
        return session.Events.AggregateStreamAsync<TAggregate>(id);
    }

    public async Task<bool> Exists<TAggregate>(string id)
        where TAggregate : AggregateBase
    {
        var streamState = await session.Events.FetchStreamStateAsync(id);
        if (streamState is null) return false;
        return !streamState.IsArchived;
    }

    public async ValueTask DisposeAsync()
    {
        logger.LogDebug("[{TransactionId}] Disposing aggregate transaction for tenant {TenantId}", transactionId, session.TenantId);
        await session.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}