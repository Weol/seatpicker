using Seatpicker.Domain;

namespace Seatpicker.Application.Features.Reservation;

public class ReservationManagementService(
    IUserProvider userProvider,
    IAggregateTransaction transaction,
    IDocumentReader reader,
    IReservationNotifier reservationNotifier)
{
    public async Task Create(string lanId, string seatId, string userId, User user)
    {
        var userToReserveFor =
            await userProvider.GetById(userId) ?? throw new UserNotFoundException { UserId = userId };

        var seatToReserve = await transaction.Aggregate<Seat>(seatId) ??
            throw new SeatNotFoundException { SeatId = seatId };

        var numReservedSeatsByUser = reader.Query<ProjectedSeat>()
            .Where(seat => seat.LanId == lanId)
            .Count(seat => seat.ReservedBy != null && seat.ReservedBy == userId);

        seatToReserve.MakeReservationFor(userToReserveFor, numReservedSeatsByUser, user);

        transaction.Update(seatToReserve);

        await reservationNotifier.NotifySeatReservationChanged(seatToReserve);
    }

    public async Task Delete(string lanId, string seatId, User user)
    {
        var seat = await transaction.Aggregate<Seat>(seatId) ?? throw new SeatNotFoundException { SeatId = seatId };

        seat.RemoveReservationFor(user);

        transaction.Update(seat);
        
        await reservationNotifier.NotifySeatReservationChanged(seat);
    }

    public async Task Move(string lanId, string fromSeatId, string toSeatId, User user)
    {
        var fromSeat = await transaction.Aggregate<Seat>(fromSeatId) ??
            throw new SeatNotFoundException { SeatId = fromSeatId };

        var toSeat = await transaction.Aggregate<Seat>(toSeatId) ??
            throw new SeatNotFoundException { SeatId = toSeatId };

        toSeat.MoveReservationFor(fromSeat, user);

        transaction.Update(fromSeat);
        transaction.Update(toSeat);
        
        await reservationNotifier.NotifySeatReservationChanged(fromSeat);
        await reservationNotifier.NotifySeatReservationChanged(toSeat);
    }
}