using Shared;

namespace Seatpicker.Domain;

public class Lan : AggregateBase
{
    public Lan(Guid lanId, string title, byte[] background, User initiator)
    {
        if (title.Length <= 0) throw new ArgumentOutOfRangeException(nameof(title), title, "Title cannot be empty");

        var evt = new LanCreated(lanId, title, background, initiator.UserId);

        Raise(evt);
        Apply(evt);
    }

    private Lan()
    {
        // Marten needs this
    }

    public string Title { get; private set; }

    public byte[] Background { get; private set; }

    public override string ToString() => $"Lan {Title} ({Id})";

    public void ChangeBackground(byte[] newBackground, User initiator)
    {
        var evt = new LanBackgroundChanged(newBackground, initiator.UserId);
        Raise(evt);
        Apply(evt);
    }

    public void ChangeTitle(string newTitle, User initiator)
    {
        var evt = new LanTitleChanged(newTitle, initiator.UserId);
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
public record LanCreated(Guid LanId, string Title, byte[] Background, UserId Initiator);

public record LanTitleChanged(string Title, UserId Initiator);

public record LanBackgroundChanged(byte[] Background, UserId Initiator);