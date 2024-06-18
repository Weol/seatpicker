using System.Diagnostics.CodeAnalysis;
using Shared;

namespace Seatpicker.Domain;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable NotAccessedPositionalProperty.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedParameter.Local
#pragma warning disable CS1998 // Disable warning about async methods missing awaits
#pragma warning disable CS8618 // Disable warning about uninitialized properties
[SuppressMessage("Performance", "CA1822:Mark members as static")]
public class Lan : AggregateBase
{
    public Lan(string lanId, string title, byte[] background, User initiator)
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

    public bool Active { get; private set; }

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

    public void SetActive(bool active, User initiator)
    {
        var evt = new LanActiveChanged(active, initiator.Id);
        Raise(evt);
        Apply(evt);
    }


    public void Archive(User initiator)
    {
        var evt = new LanArchived(initiator.Id);
        Raise(evt);
        Apply(evt);
    }

    private void Apply(LanCreated evt)
    {
        Id = evt.Id;
        Title = evt.Title;
        Background = evt.Background;
        Active = false;
    }

    private void Apply(LanTitleChanged evt)
    {
        Title = evt.Title;
    }

    private void Apply(LanBackgroundChanged evt)
    {
        Background = evt.Background;
    }

    private void Apply(LanActiveChanged evt)
    {
        Active = evt.Active;
    }

    private void Apply(LanArchived evt)
    {
    }
}

/**
 * Events
 */
public record LanCreated(string Id, string Title, byte[] Background, string CreatedBy) : IEvent;

public record LanTitleChanged(string Title, string ChangedBy) : IEvent;

public record LanBackgroundChanged(byte[] Background, string ChangedBy) : IEvent;

public record LanActiveChanged(bool Active, string ChangedBy) : IEvent;

public record LanArchived(string ArchivedBy) : IEvent;