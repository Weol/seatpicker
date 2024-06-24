using Seatpicker.Domain;

namespace Seatpicker.Application.Features.Reservation;

public class ReservationService
{
    private readonly IAggregateTransaction _aggregateTransaction;
    private readonly IDocumentReader _documentReader;

    internal ReservationService(IAggregateTransaction aggregateTransaction, IDocumentReader documentReader)
    {
        _aggregateTransaction = aggregateTransaction;
        _documentReader = documentReader;
    }

    public async Task Create(string lanId, string seatId, User user)
    {
        var seatToReserve = await _aggregateTransaction.Aggregate<Seat>(seatId) ??
                            throw new SeatNotFoundException { SeatId = seatId };

        var numReservedSeatsByUser = _documentReader.Query<ProjectedSeat>()
            .Where(seat => seat.LanId == lanId)
            .Count(seat => seat.ReservedBy != null && seat.ReservedBy == user.Id);

        seatToReserve.MakeReservation(user, numReservedSeatsByUser);

        _aggregateTransaction.Update(seatToReserve);
    }

   public async Task Remove(string seatId, User user)
    {
        var seat = await _aggregateTransaction.Aggregate<Seat>(seatId) ?? throw new SeatNotFoundException { SeatId = seatId };

        seat.RemoveReservation(user);

        _aggregateTransaction.Update(seat);
    }

   public async Task Move(string fromSeatId, string toSeatId, User user)
    {
        var fromSeat = await _aggregateTransaction.Aggregate<Seat>(fromSeatId) ??
                       throw new SeatNotFoundException { SeatId = fromSeatId };

        var toSeat = await _aggregateTransaction.Aggregate<Seat>(toSeatId) ??
                     throw new SeatNotFoundException { SeatId = toSeatId };

        toSeat.MoveReservation(user, fromSeat);

        _aggregateTransaction.Update(fromSeat);
        _aggregateTransaction.Update(toSeat);
        await _aggregateTransaction.Commit();
    }
}