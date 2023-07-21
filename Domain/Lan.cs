using Shared;

namespace Seatpicker.Domain;

public class Lan : AggregateBase
{
    public Lan(Guid lanId, string title, byte[] background)
    {
        var evt = new LanCreated(lanId, title, background);
        Raise(evt);
        Apply(evt);
    }

    private Lan()
    {
    }

    public string Title { get; private set; }

    public byte[] Background { get; private set; }

    public void ChangeBackground(byte[] newBackground)
    {
        var evt = new LanBackgroundChanged(newBackground);
        Raise(evt);
        Apply(evt);
    }

    public void ChangeTitle(string newTitle)
    {
        var evt = new LanTitleChanged(newTitle);
        Raise(evt);
        Apply(evt);
    }

    public void Apply(LanCreated evt)
    {
        Id = evt.LanId;
        Title = evt.Title;
        Background = evt.Background;
    }

    public void Apply(LanTitleChanged evt)
    {
        Title = evt.Title;
    }

    public void Apply(LanBackgroundChanged evt)
    {
        Background = evt.Background;
    }
}

/**
 * Events
 */
public record LanCreated(Guid LanId, string Title, byte[] Background);

public record LanTitleChanged(string Title);

public record LanBackgroundChanged(byte[] Background);