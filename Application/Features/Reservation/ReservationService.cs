using Seatpicker.Domain;

namespace Seatpicker.Application.Features.Reservation;

public class ReservationService(IAggregateTransaction aggregateTransaction, IDocumentReader documentReader, IReservationNotifier reservationNotifier)
{
    public async Task Create(string lanId, string seatId, User user)
    {
        var seatToReserve = await aggregateTransaction.Aggregate<Seat>(seatId) ??
                            throw new SeatNotFoundException { SeatId = seatId };

        var numReservedSeatsByUser = documentReader.Query<ProjectedSeat>()
            .Where(seat => seat.LanId == lanId)
            .Count(seat => seat.ReservedBy != null && seat.ReservedBy == user.Id);

        seatToReserve.MakeReservation(user, numReservedSeatsByUser);

        aggregateTransaction.Update(seatToReserve);
        
        await reservationNotifier.NotifySeatReservationChanged(seatToReserve);
    }

   public async Task Remove(string seatId, User user)
    {
        var seat = await aggregateTransaction.Aggregate<Seat>(seatId) ?? throw new SeatNotFoundException { SeatId = seatId };

        seat.RemoveReservation(user);
        
        aggregateTransaction.Update(seat);
        
        await reservationNotifier.NotifySeatReservationChanged(seat);
    }

   public async Task Move(string fromSeatId, string toSeatId, User user)
    {
        var fromSeat = await aggregateTransaction.Aggregate<Seat>(fromSeatId) ??
                       throw new SeatNotFoundException { SeatId = fromSeatId };

        var toSeat = await aggregateTransaction.Aggregate<Seat>(toSeatId) ??
                     throw new SeatNotFoundException { SeatId = toSeatId };

        toSeat.MoveReservation(user, fromSeat);

        aggregateTransaction.Update(fromSeat);
        aggregateTransaction.Update(toSeat);
        
        await reservationNotifier.NotifySeatReservationChanged(fromSeat);
        await reservationNotifier.NotifySeatReservationChanged(toSeat);
    }
}