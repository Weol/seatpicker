namespace Shared;

public abstract class AggregateBase
{
    public Guid Id { get; set; }

    public IList<object> RaisedEvents = new List<object>();

    protected void Raise(object evt)
    {
        RaisedEvents.Add(evt);
    }
}