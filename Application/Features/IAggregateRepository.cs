using Shared;

namespace Seatpicker.Application.Features;

public interface IAggregateRepository
{
    public IAggregateTransaction CreateTransaction();

    public IAggregateReader CreateReader();
}

public interface IAggregateReader : IAsyncDisposable
{
    public Task<TAggregate?> Aggregate<TAggregate>(Guid id)
        where TAggregate : AggregateBase;

    public Task<bool> Exists<TAggregate>(Guid id)
        where TAggregate : AggregateBase;

    public IQueryable<TAggregate> Query<TAggregate>()
        where TAggregate : AggregateBase;
}

public interface IAggregateTransaction : IAggregateReader, IAsyncDisposable
{
    public void Update<TAggregate>(TAggregate aggregate)
        where TAggregate : AggregateBase;

    public void Create<TAggregate>(TAggregate aggregate)
        where TAggregate : AggregateBase;

    public void Archive<TAggregate>(TAggregate aggregate)
        where TAggregate : AggregateBase;

    public void Commit();
}