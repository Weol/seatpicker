using Seatpicker.Application.Features.Lan;
using Seatpicker.Domain;

namespace Seatpicker.Application.Features.Reservation;

public class SeatManagementService(IAggregateTransaction aggregateTransaction, User actor)
{
    public async Task UpdateTitle(string seatId, string title)
    {
        var seat = await aggregateTransaction.Aggregate<Seat>(seatId) ?? throw new SeatNotFoundException{ SeatId = seatId };

         seat.SetTitle(title, actor);

        aggregateTransaction.Update(seat);
    }

    public async Task UpdateBounds(string seatId, Bounds bounds)
    {
        var seat = await aggregateTransaction.Aggregate<Seat>(seatId) ?? throw new SeatNotFoundException{ SeatId = seatId };

        seat.SetBounds(bounds, actor);

        aggregateTransaction.Update(seat);
    }

   public async Task<string> Create(string lanId, string title, Bounds bounds)
    {
        var id = Guid.NewGuid().ToString();

        var lan = await aggregateTransaction.Aggregate<Domain.Lan>(lanId)
            ?? throw new LanNotFoundException { LanId = lanId };

        var seat = new Seat(id, lan, title, bounds, actor);

        aggregateTransaction.Create(seat);

        return id;
    }

   public async Task Remove(string seatId)
    {
        var seat = await aggregateTransaction.Aggregate<Seat>(seatId) ?? throw new SeatNotFoundException{ SeatId = seatId };

        seat.Archive(actor);

        aggregateTransaction.Update(seat);
        aggregateTransaction.Archive(seat);
    }
}