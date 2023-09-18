namespace Shared;

public interface IEvent
{
    public DateTimeOffset Timestamp { get; set; }
}