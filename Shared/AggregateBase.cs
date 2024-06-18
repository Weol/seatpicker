using System.Text.Json.Serialization;

namespace Shared;

public abstract class AggregateBase
{
    public string Id { get; set; }

    [JsonIgnore]
    public IList<object> RaisedEvents { get; } = new List<object>();

    protected void Raise(object evt)
    {
        RaisedEvents.Add(evt);
    }
}