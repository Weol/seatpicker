using Marten.Events;
using Marten.Events.Aggregation;
using Microsoft.CodeAnalysis;
using Seatpicker.Domain;
using Shared;

namespace Seatpicker.Application.Features.Lans;

public class LanProjection : SingleStreamProjection<ProjectedLan>
{
    public LanProjection()
    {
        DeleteEvent<LanArchived>();
    }

    public ProjectedLan Create(IEvent<LanCreated> evt)
    {
        return new ProjectedLan(evt.Data.Id, evt.Data.GuildId, evt.Data.Title, evt.Data.Background, evt.Timestamp, evt.Timestamp);
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

public class ProjectedLan: IDocument
{
    public ProjectedLan(Guid id, string guildId, string title, byte[] background, DateTimeOffset createdAt, DateTimeOffset updatedAt)
    {
        Id = id;
        GuildId = guildId;
        Title = title;
        Active = false;
        Background = background;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public Guid Id { get; set; }
    public string GuildId { get; set; }
    public string Title { get; set; }
    public byte[] Background { get; set; }
    public bool Active { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
