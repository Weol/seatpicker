using Seatpicker.Application.Features.Lan;
using Seatpicker.Domain;

namespace Seatpicker.Application.Features.Reservation;

public class SeatManagementService(IAggregateTransaction aggregateTransaction)
{
    public async Task UpdateTitle(string seatId, string title, User user)
    {
        var seat = await aggregateTransaction.Aggregate<Seat>(seatId) ??
                   throw new SeatNotFoundException { SeatId = seatId };

        seat.SetTitle(title, user);

        aggregateTransaction.Update(seat);
    }

    public async Task UpdateBounds(string seatId, Bounds bounds, User user)
    {
        var seat = await aggregateTransaction.Aggregate<Seat>(seatId) ??
                   throw new SeatNotFoundException { SeatId = seatId };

        seat.SetBounds(bounds, user);

        aggregateTransaction.Update(seat);
    }

    public async Task<string> Create(string lanId, string title, Bounds bounds, User user)
    {
        var id = Guid.NewGuid().ToString();

        var lan = await aggregateTransaction.Aggregate<Domain.Lan>(lanId) ??
                  throw new LanNotFoundException { LanId = lanId };

        var seat = new Seat(id, lan, title, bounds, user);

        aggregateTransaction.Create(seat);

        return id;
    }

    public async Task Remove(string seatId, User user)
    {
        var seat = await aggregateTransaction.Aggregate<Seat>(seatId) ??
                   throw new SeatNotFoundException { SeatId = seatId };

        seat.Archive(user);

        aggregateTransaction.Update(seat);
        aggregateTransaction.Archive(seat);
    }
}