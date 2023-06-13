namespace Shared;

public abstract class Entity
{
    public ICollection<IDomainEvent> RaisedEvents { get; } = new List<IDomainEvent>();

    protected void Raise(IDomainEvent domainEvent) => RaisedEvents.Add(domainEvent);
}