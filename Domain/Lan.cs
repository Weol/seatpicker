using Shared;

namespace Seatpicker.Domain;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable NotAccessedPositionalProperty.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedParameter.Local
#pragma warning disable CS1998 // Disable warning about async methods missing awaits
#pragma warning disable CS8618 // Disable warning about uninitialized properties
public class Lan : AggregateBase
{
    public Lan(Guid lanId, string title, byte[] background, User initiator)
    {
        if (title.Length <= 0) throw new ArgumentOutOfRangeException(nameof(title), title, "Title cannot be empty");

        var evt = new LanCreated(lanId, title, background, initiator.Id);

        Raise(evt);
        Apply(evt);
    }

    // ReSharper disable once UnusedMember.Local
    private Lan()
    {
        // Marten needs this
    }

    public string Title { get; private set; }

    public byte[] Background { get; private set; }

    public override string ToString() => $"Lan {Title} ({Id})";

    public void ChangeBackground(byte[] newBackground, User initiator)
    {
        var evt = new LanBackgroundChanged(newBackground, initiator.Id);
        Raise(evt);
        Apply(evt);
    }

    public void ChangeTitle(string newTitle, User initiator)
    {
        var evt = new LanTitleChanged(newTitle, initiator.Id);
        Raise(evt);
        Apply(evt);
    }

    public void Apply(LanCreated evt)
    {
        Id = evt.Id;
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
public record LanCreated(Guid Id, string Title, byte[] Background, UserId Initiator);

public record LanTitleChanged(string Title, UserId Initiator);

public record LanBackgroundChanged(byte[] Background, UserId Initiator);