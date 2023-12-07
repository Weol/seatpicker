using Marten.Events;
using Marten.Events.Projections;
using Seatpicker.Domain;
using Shared;

namespace Seatpicker.Application.Features;

public class LanEventsProjection : MultiStreamProjection<LanEventsDocument, Guid>
{
    public LanEventsProjection()
    {
        DeleteEvent<LanArchived>();

        Identity<LanCreated>(e => e.Id);
        Identity<IEvent<LanActiveChanged>>(e => e.StreamId);
        Identity<IEvent<LanTitleChanged>>(e => e.StreamId);
        Identity<IEvent<LanBackgroundChanged>>(e => e.StreamId);
        Identity<SeatCreated>(e => e.LanId);
        Identity<SeatBoundsChanged>(e => e.LanId);
        Identity<SeatTitleChanged>(e => e.LanId);
        Identity<SeatReservationMade>(e => e.LanId);
        Identity<SeatReservationMoved>(e => e.LanId);
        Identity<SeatReservationRemoved>(e => e.LanId);
        Identity<SeatReservationMadeFor>(e => e.LanId);
        Identity<SeatReservationMovedFor>(e => e.LanId);
        Identity<SeatReservationRemovedFor>(e => e.LanId);
        Identity<SeatArchived>(e => e.LanId);
    }

    public LanEventsProjection Create(SeatCreated evt)
    {
        return new LanEventsProjection
        {
            Events = new List<string>()
        };
    }

    public void Apply(IEvent<LanActiveChanged> evt, LanEventsProjection projection)
    {

    }

    public void Apply(IEvent<LanTitleChanged> evt, LanEventsProjection projection)
    {
    }

    public void Apply(IEvent<LanBackgroundChanged> evt, LanEventsProjection projection)
    {
    }

    public void Apply(IEvent<SeatCreated> evt, LanEventsProjection projection)
    {
    }

    public void Apply(IEvent<SeatBoundsChanged> evt, LanEventsProjection projection)
    {
    }

    public void Apply(IEvent<SeatTitleChanged> evt, LanEventsProjection projection)
    {
    }

    public void Apply(IEvent<SeatReservationMade> evt, LanEventsProjection projection)
    {
    }

    public void Apply(IEvent<SeatReservationMoved> evt, LanEventsProjection projection)
    {
    }

    public void Apply(IEvent<SeatReservationRemoved> evt, LanEventsProjection projection)
    {
    }

    public void Apply(IEvent<SeatReservationMadeFor> evt, LanEventsProjection projection)
    {
    }

    public void Apply(IEvent<SeatReservationMovedFor> evt, LanEventsProjection projection)
    {
    }

    public void Apply(IEvent<SeatReservationRemovedFor> evt), LanEventsProjection projection
    {
    }

    public void Apply(IEvent<SeatArchived> evt, LanEventsProjection projection)
    {
    }
}

public class LanEventsDocument : IDocument
{
    public IList<string> Events { get; init; }
}