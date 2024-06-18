using Marten;
using Seatpicker.Application.Features;
using Shared;

namespace Seatpicker.Infrastructure.Adapters.Database;

public class AggregateRepository(IDocumentStore store) : IAggregateRepository
{
    public IAggregateTransaction CreateTransaction(string guildId)
    {
        var session = store.LightweightSession(guildId);
        return new AggregateTransaction(session);
    }

    public IGuildlessAggregateTransaction CreateGuildlessTransaction()
    {
        var session = store.LightweightSession();
        return new AggregateTransaction(session);
    }
}

public class AggregateTransaction : IGuildlessAggregateTransaction
{
    private readonly IDocumentSession session;

    public AggregateTransaction(IDocumentSession session)
    {
        this.session = session;
    }

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