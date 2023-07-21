using Marten;
using Seatpicker.Application.Features;
using Shared;

namespace Seatpicker.IntegrationTests.TestAdapters;

#pragma warning disable CS1998 //Async method lacks await

public class TestAggregateRepository : IAggregateRepository
{
    public IDictionary<Guid, AggregateBase> Aggregates { get; }= new Dictionary<Guid, AggregateBase>();

    public IAggregateTransaction CreateTransaction()
    {
        return new TestAggregateTransaction(Aggregates);
    }

    public IAggregateReader CreateReader()
    {
        return new TestAggregateReader(Aggregates);
    }
}

public class TestAggregateTransaction : IAggregateTransaction
{
    private readonly IDictionary<Guid, AggregateBase> aggregates;

    private readonly TestAggregateReader reader;
    private readonly IList<AggregateBase> stagedAggregates = new List<AggregateBase>();

    public TestAggregateTransaction(IDictionary<Guid,AggregateBase> aggregates)
    {
        this.aggregates = aggregates;
        reader = new TestAggregateReader(aggregates);
    }

    public void Update<TAggregate>(TAggregate aggregate)
        where TAggregate : AggregateBase
    {
        if (!aggregate.RaisedEvents.Any()) throw new NoRaisedEventsException { Id = aggregate.Id, Type = typeof(TAggregate) };

        var exists = Exists<TAggregate>(aggregate.Id).GetAwaiter().GetResult();
        if (!exists) throw new AggregateDoesNotExistException { Id = aggregate.Id, Type = typeof(TAggregate)};

        stagedAggregates.Add(aggregate);
    }

    public void Create<TAggregate>(TAggregate aggregate)
        where TAggregate : AggregateBase
    {
        if (!aggregate.RaisedEvents.Any()) throw new NoRaisedEventsException { Id = aggregate.Id, Type = typeof(TAggregate) };

        var exists = Exists<TAggregate>(aggregate.Id).GetAwaiter().GetResult();
        if (exists) throw new AggregateAlreadyExistsException{ Id = aggregate.Id, Type = typeof(TAggregate)};

        stagedAggregates.Add(aggregate);
    }

    public void Commit()
    {
        foreach (var aggregate in stagedAggregates)
        {
            aggregates[aggregate.Id] = aggregate;
        }
    }

    public Task<TAggregate?> Aggregate<TAggregate>(Guid id)
        where TAggregate : AggregateBase =>
        reader.Aggregate<TAggregate>(id);

    public Task<bool> Exists<TAggregate>(Guid id)
        where TAggregate : AggregateBase =>
        reader.Exists<TAggregate>(id);

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}

public class TestAggregateReader : IAggregateReader
{
    private readonly IDictionary<Guid, AggregateBase> aggregates;

    public TestAggregateReader(IDictionary<Guid, AggregateBase> aggregates)
    {
        this.aggregates = aggregates;
    }

    public async Task<TAggregate?> Aggregate<TAggregate>(Guid id)
        where TAggregate : AggregateBase
    {
        if (!aggregates.TryGetValue(id, out var aggregate)) return null;

        if (aggregate is not TAggregate typedAggregate) throw new TypeMismatchException
        {
            Id = id,
            ExpectedType = typeof(TAggregate),
            ActualType = aggregate.GetType(),
        };

        return typedAggregate;
    }

    public async Task<bool> Exists<TAggregate>(Guid id)
        where TAggregate : AggregateBase
    {
        if (!aggregates.TryGetValue(id, out var aggregate)) return false;

        if (aggregate is not TAggregate) throw new TypeMismatchException
        {
            Id = id,
            ExpectedType = typeof(TAggregate),
            ActualType = aggregate.GetType(),
        };

        return true;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}

public class TypeMismatchException : Exception
{
    public required Guid Id { get; init; }
    public required Type ExpectedType { get; init; }
    public required Type ActualType { get; init; }

    public override string Message =>
        $"Type mismatch in test store, expected aggregate with id {Id} to be of type {ExpectedType.Name} but was actually {ActualType.Name}";
}

public class NoRaisedEventsException : Exception
{
    public required Guid Id { get; init; }
    public required Type Type { get; init; }

    public override string Message =>
        $"Aggregate of type {Type.Name} and id {Id} must have at least one raised event";
}

public class AggregateAlreadyExistsException : Exception
{
    public required Guid Id { get; init; }
    public required Type Type { get; init; }

    public override string Message =>
        $"Aggregate of type {Type.Name} and id {Id} already exists in test store";
}

public class AggregateDoesNotExistException : Exception
{
    public required Guid Id { get; init; }
    public required Type Type { get; init; }

    public override string Message =>
        $"Aggregate of type {Type.Name} and id {Id} dose not exist in test store";
}
