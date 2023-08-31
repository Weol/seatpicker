using Marten;
using NSubstitute;
using Seatpicker.Application.Features;
using Shared;

namespace Seatpicker.IntegrationTests.TestAdapters;

#pragma warning disable CS1998 //Async method lacks await

public class TestAggregateRepository : IAggregateRepository
{
    public IDictionary<Guid, (AggregateBase Aggregate, bool IsArchived)> Aggregates { get; }
        = new Dictionary<Guid, (AggregateBase Aggregate, bool IsArchived)>();

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
    private readonly IDictionary<Guid, (AggregateBase Aggregate, bool IsArchived)> aggregates;

    private readonly TestAggregateReader reader;
    private readonly IList<AggregateBase> stagedAggregates = new List<AggregateBase>();
    private readonly IList<AggregateBase> stagedForArchive = new List<AggregateBase>();

    public TestAggregateTransaction(IDictionary<Guid,(AggregateBase Aggregate, bool IsArchived)> aggregates)
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

        // we don't use the Exists() method because we need to fail if we try to create an aggregate that has the
        // same id as an archived aggregate.
        var exists = aggregates.Values.Any(tuple => tuple.Aggregate.Id == aggregate.Id);
        if (exists) throw new AggregateAlreadyExistsException{ Id = aggregate.Id, Type = typeof(TAggregate)};

        stagedAggregates.Add(aggregate);
    }

    public void Archive<TAggregate>(TAggregate aggregate)
        where TAggregate : AggregateBase
    {
        var exists = Exists<TAggregate>(aggregate.Id).GetAwaiter().GetResult();
        if (!exists) throw new AggregateDoesNotExistException { Id = aggregate.Id, Type = typeof(TAggregate)};

        stagedForArchive.Add(aggregate);
    }

    public void Commit()
    {
        foreach (var aggregate in stagedAggregates)
        {
            aggregates[aggregate.Id] = (aggregate, false);
        }

        foreach (var aggregate in stagedForArchive)
        {
            aggregates[aggregate.Id] = (aggregate, true);
        }
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

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}

public class TestAggregateReader : IAggregateReader
{
    private readonly IDictionary<Guid, (AggregateBase Aggregate, bool IsArchived)> aggregates;

    public TestAggregateReader(IDictionary<Guid, (AggregateBase Aggregate, bool IsArchived)> aggregates)
    {
        this.aggregates = aggregates;
    }

    public async Task<TAggregate?> Aggregate<TAggregate>(Guid id)
        where TAggregate : AggregateBase
    {
        if (!aggregates.TryGetValue(id, out var tuple)) return null;

        if (tuple.Aggregate is not TAggregate typedAggregate) throw new TypeMismatchException
        {
            Id = id,
            ExpectedType = typeof(TAggregate),
            ActualType = tuple.Aggregate.GetType(),
        };

        return tuple.IsArchived ? null : typedAggregate;
    }

    public async Task<bool> Exists<TAggregate>(Guid id)
        where TAggregate : AggregateBase
    {
        return await Aggregate<TAggregate>(id) is not null;
    }

    public IQueryable<TAggregate> Query<TAggregate>()
        where TAggregate : AggregateBase
    {
        return aggregates.Values
            .Where(tuple => !tuple.IsArchived)
            .Select(tuple => tuple.Aggregate)
            .OfType<TAggregate>().AsQueryable();
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
