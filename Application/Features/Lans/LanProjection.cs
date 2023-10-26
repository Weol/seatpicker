using Marten.Events.Aggregation;
using Seatpicker.Domain;
using Shared;

namespace Seatpicker.Application.Features.Lans;

public class LanProjection : SingleStreamProjection<ProjectedLan>
{
    public LanProjection()
    {
        DeleteEvent<SeatArchived>();
    }

    public ProjectedLan Create(LanCreated evt)
    {
        return new ProjectedLan(evt.Id, evt.Title, evt.Background);
    }

    public void Apply(LanBackgroundChanged evt, ProjectedLan lan)
    {
        lan.Background = evt.Background;
    }

    public void Apply(LanTitleChanged evt, ProjectedLan lan)
    {
        lan.Title = evt.Title;
    }
}

public class ProjectedLan: IDocument
{
    public ProjectedLan(Guid id, string title, byte[] background)
    {
        Id = id;
        Title = title;
        Background = background;
    }

    public Guid Id { get; set; }
    public string Title { get; set; }
    public byte[] Background { get; set; }
}
