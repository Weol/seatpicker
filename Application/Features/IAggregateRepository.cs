using Marten;
using Shared;

namespace Seatpicker.Application.Features;

public interface IAggregateRepository
{
    public IAggregateTransaction CreateTransaction(string guildId, IDocumentSession? documentSession = null);

    public IGuildlessAggregateTransaction CreateGuildlessTransaction(IDocumentSession? documentSession = null);
}

public interface IGuildlessAggregateTransaction : IAggregateTransaction;

public interface IAggregateTransaction : IDisposable
{
    public void Update<TAggregate>(TAggregate aggregate)
        where TAggregate : AggregateBase;

    public void Create<TAggregate>(TAggregate aggregate)
        where TAggregate : AggregateBase;

    public void Archive<TAggregate>(TAggregate aggregate)
        where TAggregate : AggregateBase;

    public Task<TAggregate?> Aggregate<TAggregate>(string id)
        where TAggregate : AggregateBase;

    public Task<bool> Exists<TAggregate>(string id)
        where TAggregate : AggregateBase;

    public Task Commit();
}