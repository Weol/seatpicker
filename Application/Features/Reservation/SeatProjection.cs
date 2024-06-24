using System.Diagnostics.CodeAnalysis;
using Marten.Events.Aggregation;
using Seatpicker.Domain;
using Shared;

namespace Seatpicker.Application.Features.Reservation;

[SuppressMessage("Performance", "CA1822:Mark members as static")]
public class SeatProjection : SingleStreamProjection<ProjectedSeat>
{
    public SeatProjection()
    {
        DeleteEvent<SeatArchived>();
    }

    public ProjectedSeat Create(SeatCreated evt)
    {
        return new ProjectedSeat(evt.Id, evt.LanId, evt.Title, evt.Bounds, null);
    }

    public void Apply(SeatReservationMade evt, ProjectedSeat seat)
    {
        seat.ReservedBy = evt.UserId;
    }

    public void Apply(SeatReservationMadeFor evt, ProjectedSeat seat)
    {
        seat.ReservedBy = evt.UserId;
    }

    public void Apply(SeatReservationRemoved evt, ProjectedSeat seat)
    {
        seat.ReservedBy = null;
    }

    public void Apply(SeatReservationRemovedFor evt, ProjectedSeat seat)
    {
        seat.ReservedBy = null;
    }

    public void Apply(SeatReservationMoved evt, ProjectedSeat seat)
    {
        if (evt.ToSeatId == seat.Id)
            seat.ReservedBy = evt.UserId;
        else if (evt.FromSeatId == seat.Id)
            seat.ReservedBy = null;
    }

    public void Apply(SeatReservationMovedFor evt, ProjectedSeat seat)
    {
       if (evt.ToSeatId == seat.Id)
            seat.ReservedBy = evt.UserId;
       else if (evt.FromSeatId == seat.Id)
            seat.ReservedBy = null;
    }

    public void Apply(SeatTitleChanged evt, ProjectedSeat seat)
    {
        seat.Title = evt.Title;
    }

    public void Apply(SeatBoundsChanged evt, ProjectedSeat seat)
    {
        seat.Bounds = evt.Bounds;
    }
}

public class ProjectedSeat(string id, string lanId, string title, Bounds bounds, string? reservedBy) : IDocument
{
    public string Id { get; set; } = id;
    public string LanId { get; set; } = lanId;
    public string Title { get; set; } = title;
    public Bounds Bounds { get; set; } = bounds;
    public string? ReservedBy { get; set; } = reservedBy;
}
