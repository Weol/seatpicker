using Seatpicker.Application.Features.Lans;
using Seatpicker.Domain;

namespace Seatpicker.Application.Features.Seats;

public class SeatManagementService(IAggregateTransaction aggregateTransaction, User initiator)
{
    public async Task UpdateTitle(string seatId, string title)
    {
        var seat = await aggregateTransaction.Aggregate<Seat>(seatId) ?? throw new SeatNotFoundException{ SeatId = seatId };

         seat.SetTitle(title, initiator);

        aggregateTransaction.Update(seat);
    }

    public async Task UpdateBounds(string seatId, Bounds bounds)
    {
        var seat = await aggregateTransaction.Aggregate<Seat>(seatId) ?? throw new SeatNotFoundException{ SeatId = seatId };

        seat.SetBounds(bounds, initiator);

        aggregateTransaction.Update(seat);
    }

   public async Task<string> Create(string lanId, string title, Bounds bounds)
    {
        var id = Guid.NewGuid().ToString();

        var lan = await aggregateTransaction.Aggregate<Lan>(lanId)
            ?? throw new LanNotFoundException { LanId = lanId };

        var seat = new Seat(id, lan, title, bounds, initiator);

        aggregateTransaction.Create(seat);

        return id;
    }

   public async Task Remove(string seatId)
    {
        var seat = await aggregateTransaction.Aggregate<Seat>(seatId) ?? throw new SeatNotFoundException{ SeatId = seatId };

        seat.Archive(initiator);

        aggregateTransaction.Update(seat);
        aggregateTransaction.Archive(seat);
    }
}