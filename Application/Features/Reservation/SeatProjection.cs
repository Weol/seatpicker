using System.Diagnostics.CodeAnalysis;
using Marten.Events;
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

    public ProjectedSeat Create(IEvent<SeatCreated> evt)
    {
        return new ProjectedSeat(evt.Data.Id, evt.Data.LanId, evt.Data.Title, evt.Data.Bounds, null, evt.Timestamp, evt.Timestamp);
    }

    public void Apply(IEvent<SeatReservationMade> evt, ProjectedSeat seat)
    {
        seat.ReservedBy = evt.Data.UserId;
        seat.UpdatedAt = evt.Timestamp;
    }

    public void Apply(IEvent<SeatReservationMadeFor> evt, ProjectedSeat seat)
    {
        seat.ReservedBy = evt.Data.UserId;
        seat.UpdatedAt = evt.Timestamp;
    }

    public void Apply(IEvent<SeatReservationRemoved> evt, ProjectedSeat seat)
    {
        seat.ReservedBy = null;
        seat.UpdatedAt = evt.Timestamp;
    }

    public void Apply(IEvent<SeatReservationRemovedFor> evt, ProjectedSeat seat)
    {
        seat.ReservedBy = null;
        seat.UpdatedAt = evt.Timestamp;
    }

    public void Apply(IEvent<SeatReservationMoved> evt, ProjectedSeat seat)
    {
        if (evt.Data.ToSeatId == seat.Id)
        {
            seat.ReservedBy = evt.Data.UserId;
            seat.UpdatedAt = evt.Timestamp;
        }
        else if (evt.Data.FromSeatId == seat.Id)
        {
            seat.ReservedBy = null;
            seat.UpdatedAt = evt.Timestamp;
        }
    }

    public void Apply(IEvent<SeatReservationMovedFor> evt, ProjectedSeat seat)
    {
        if (evt.Data.ToSeatId == seat.Id)
        {
            seat.ReservedBy = evt.Data.UserId;
            seat.UpdatedAt = evt.Timestamp;
        }
        else if (evt.Data.FromSeatId == seat.Id)
        {
            seat.ReservedBy = null;
            seat.UpdatedAt = evt.Timestamp;
        }
    }

    public void Apply(IEvent<SeatTitleChanged> evt, ProjectedSeat seat)
    {
        seat.Title = evt.Data.Title;
        seat.UpdatedAt = evt.Timestamp;
    }

    public void Apply(IEvent<SeatBoundsChanged> evt, ProjectedSeat seat)
    {
        seat.Bounds = evt.Data.Bounds;
        seat.UpdatedAt = evt.Timestamp;
    }
}

public class ProjectedSeat(
    string id,
    string lanId,
    string title,
    Bounds bounds,
    string? reservedBy,
    DateTimeOffset createdAt,
    DateTimeOffset updatedAt) : IDocument
{
    public string Id { get; set; } = id;
    public string LanId { get; set; } = lanId;
    public string Title { get; set; } = title;
    public Bounds Bounds { get; set; } = bounds;
    public string? ReservedBy { get; set; } = reservedBy;
    public DateTimeOffset CreatedAt { get; set; } = createdAt;
    public DateTimeOffset UpdatedAt { get; set; } = updatedAt;
}