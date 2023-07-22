using Marten;
using Seatpicker.Application.Features;
using Shared;

namespace Seatpicker.Infrastructure.Adapters.Database;

public class AggregateRepository : IAggregateRepository
{
    private readonly IDocumentStore store;

    public AggregateRepository(IDocumentStore store)
    {
        this.store = store;
    }

    public IAggregateTransaction CreateTransaction()
    {
        var session = store.LightweightSession();
        return new AggregateTransaction(session);
    }

    public IAggregateReader CreateReader()
    {
        var session = store.QuerySession();
        return new AggregateReader(session);
    }
}

public class AggregateTransaction : IAggregateTransaction
{
    private readonly IDocumentSession session;
    private readonly AggregateReader reader;

    public AggregateTransaction(IDocumentSession session)
    {
        this.session = session;
        reader = new AggregateReader(session);
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

    public void Commit()
    {
        session.SaveChanges();
    }

    public Task<TAggregate?> Aggregate<TAggregate>(Guid id)
        where TAggregate : AggregateBase =>
        reader.Aggregate<TAggregate>(id);

    public Task<bool> Exists<TAggregate>(Guid id)
        where TAggregate : AggregateBase =>
        reader.Exists<TAggregate>(id);

    public IQueryable<TAggregate> Query<TAggregate>()
        where TAggregate : AggregateBase =>
        reader.Query<TAggregate>();

    public ValueTask DisposeAsync()
    {
        return session.DisposeAsync();
    }
}

public class AggregateReader : IAggregateReader
{
    private readonly IQuerySession session;

    public AggregateReader(IQuerySession session)
    {
        this.session = session;
    }

    public Task<TAggregate?> Aggregate<TAggregate>(Guid id)
        where TAggregate : AggregateBase
    {
        return session.Events.AggregateStreamAsync<TAggregate>(id);
    }

    public async Task<bool> Exists<TAggregate>(Guid id)
        where TAggregate : AggregateBase
    {
        return await session.Events.FetchStreamStateAsync(id) is not null;
    }

    public IQueryable<TAggregate> Query<TAggregate>()
        where TAggregate : AggregateBase
    {
        return session.Query<TAggregate>();
    }

    public ValueTask DisposeAsync()
    {
        return session.DisposeAsync();
    }
}