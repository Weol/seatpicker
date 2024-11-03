using Marten;
using Marten.Storage;
using Seatpicker.Application.Features;
using Shared;

namespace Seatpicker.Infrastructure.Adapters.Database;

public class AggregateRepository(IDocumentStore store) : IAggregateRepository
{
    public IAggregateTransaction CreateTransaction(string guildId, IDocumentSession? documentSession = null)
    {
        documentSession ??= store.LightweightSession(guildId);
        return new AggregateTransaction(documentSession);
    }

    public IGuildlessAggregateTransaction CreateGuildlessTransaction(IDocumentSession? documentSession = null)
    {
        documentSession ??= store.LightweightSession();

        if (documentSession.TenantId != Tenancy.DefaultTenantId)
        {
            throw new InvalidTenantIdForGuildlessSessionException {
                TenantId = documentSession.TenantId,
            };
        }

        return new AggregateTransaction(documentSession);
    }

    public class InvalidTenantIdForGuildlessSessionException : Exception
    {
        public string TenantId { get; init; }
    };
}

public class AggregateTransaction(IDocumentSession session) : IGuildlessAggregateTransaction
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

    public void Dispose()
    {
        session.Dispose();
        GC.SuppressFinalize(this);
    }
}