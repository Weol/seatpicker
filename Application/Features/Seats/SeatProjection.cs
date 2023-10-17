using Marten.Events.Aggregation;
using Microsoft.CodeAnalysis;
using Seatpicker.Domain;
using Shared;

namespace Seatpicker.Application.Features.Seats;

public class SeatProjection : SingleStreamProjection<ProjectedSeat>
{
    public SeatProjection()
    {
        DeleteEvent<SeatArchived>();
    }

    public ProjectedSeat Create(SeatCreated evt)
    {
        return new ProjectedSeat
        {
            Id = evt.Id,
            LanId = evt.LanId,
            Title = evt.Title,
            Bounds = evt.Bounds,
            ReservedBy = null,
        };
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
        seat.ReservedBy = evt.UserId;
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

public class ProjectedSeat : IDocument
{
    public Guid Id { get; set; }
    public Guid LanId { get; set; }
    public string Title { get; set; }
    public Bounds Bounds { get; set; }
    public UserId? ReservedBy { get; set; }
}
