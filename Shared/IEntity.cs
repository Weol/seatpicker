namespace Shared;

public abstract class Entity<TId>
{
    public TId Id { get; init; }

    public ICollection<IDomainEvent> RaisedEvents { get; } = new List<IDomainEvent>();

    protected void Raise(IDomainEvent domainEvent) => RaisedEvents.Add(domainEvent);
}