namespace Shared;

public abstract class AggregateRoot
{
    public Guid Id { get; set; }

    public IList<object> RaisedEvents = new List<object>();

    protected void Raise(object evt)
    {
        RaisedEvents.Add(evt);
    }
}