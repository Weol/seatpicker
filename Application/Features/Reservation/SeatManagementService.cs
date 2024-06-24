using Seatpicker.Application.Features.Lan;
using Seatpicker.Domain;

namespace Seatpicker.Application.Features.Reservation;

public class SeatManagementService
{
    private readonly IAggregateTransaction _aggregateTransaction;
    private readonly User _initiator;

    internal SeatManagementService(IAggregateTransaction aggregateTransaction, User initiator)
    {
        _aggregateTransaction = aggregateTransaction;
        _initiator = initiator;
    }

    public async Task UpdateTitle(string seatId, string title)
    {
        var seat = await _aggregateTransaction.Aggregate<Seat>(seatId) ?? throw new SeatNotFoundException{ SeatId = seatId };

         seat.SetTitle(title, _initiator);

        _aggregateTransaction.Update(seat);
    }

    public async Task UpdateBounds(string seatId, Bounds bounds)
    {
        var seat = await _aggregateTransaction.Aggregate<Seat>(seatId) ?? throw new SeatNotFoundException{ SeatId = seatId };

        seat.SetBounds(bounds, _initiator);

        _aggregateTransaction.Update(seat);
    }

   public async Task<string> Create(string lanId, string title, Bounds bounds)
    {
        var id = Guid.NewGuid().ToString();

        var lan = await _aggregateTransaction.Aggregate<Domain.Lan>(lanId)
            ?? throw new LanNotFoundException { LanId = lanId };

        var seat = new Seat(id, lan, title, bounds, _initiator);

        _aggregateTransaction.Create(seat);

        return id;
    }

   public async Task Remove(string seatId)
    {
        var seat = await _aggregateTransaction.Aggregate<Seat>(seatId) ?? throw new SeatNotFoundException{ SeatId = seatId };

        seat.Archive(_initiator);

        _aggregateTransaction.Update(seat);
        _aggregateTransaction.Archive(seat);
    }
}