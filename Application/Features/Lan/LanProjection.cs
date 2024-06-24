using System.Diagnostics.CodeAnalysis;
using Marten.Events;
using Marten.Events.Aggregation;
using Seatpicker.Domain;
using Shared;

namespace Seatpicker.Application.Features.Lan;

[SuppressMessage("Performance", "CA1822:Mark members as static")]
public class LanProjection : SingleStreamProjection<ProjectedLan>
{
    public LanProjection()
    {
        DeleteEvent<LanArchived>();
    }

    public ProjectedLan Create(IEvent<LanCreated> evt)
    {
        return new ProjectedLan(evt.Data.Id, evt.Data.Title, evt.Data.Background, evt.Timestamp, evt.Timestamp);
    }

    public void Apply(IEvent<LanBackgroundChanged> evt, ProjectedLan lan)
    {
        lan.Background = evt.Data.Background;
        lan.UpdatedAt = evt.Timestamp;
    }

    public void Apply(IEvent<LanTitleChanged> evt, ProjectedLan lan)
    {
        lan.Title = evt.Data.Title;
        lan.UpdatedAt = evt.Timestamp;
    }

    public void Apply(IEvent<LanActiveChanged> evt, ProjectedLan lan)
    {
        lan.Active = evt.Data.Active;
        lan.UpdatedAt = evt.Timestamp;
    }
}

public class ProjectedLan(
    string id,
    string title,
    byte[] background,
    DateTimeOffset createdAt,
    DateTimeOffset updatedAt) : IDocument
{
    public string Id { get; set; } = id;
    public string Title { get; set; } = title;
    public byte[] Background { get; set; } = background;
    public bool Active { get; set; } = false;
    public DateTimeOffset CreatedAt { get; set; } = createdAt;
    public DateTimeOffset UpdatedAt { get; set; } = updatedAt;
}
