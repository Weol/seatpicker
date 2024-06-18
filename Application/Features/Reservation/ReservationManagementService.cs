using Marten;
using Seatpicker.Application.Features.Guilds;
using Seatpicker.Domain;

namespace Seatpicker.Application.Features.Seats;

public class ReservationManagementService(
    IUserProvider userProvider,
    IAggregateTransaction transaction,
    IDocumentReader reader,
    User initiator)
{
    public async Task Create(string lanId, string seatId, string userId)
    {
        var userToReserveFor
            = await userProvider.GetById(userId) ?? throw new UserNotFoundException { UserId = userId };

        var seatToReserve = await transaction.Aggregate<Seat>(seatId) ??
            throw new SeatNotFoundException { SeatId = seatId };

        var numReservedSeatsByUser = reader.Query<ProjectedSeat>()
            .Where(seat => seat.LanId == lanId)
            .Count(seat => seat.ReservedBy != null && seat.ReservedBy == userId);

        seatToReserve.MakeReservationFor(userToReserveFor, numReservedSeatsByUser, initiator);

        transaction.Update(seatToReserve);
    }

   public async Task Delete(string lanId, string seatId)
    {
        var seat = await transaction.Aggregate<Seat>(seatId) ?? throw new SeatNotFoundException { SeatId = seatId };

        seat.RemoveReservationFor(initiator);

        transaction.Update(seat);
    }

   public async Task Move(string lanId, string fromSeatId, string toSeatId)
    {
        var fromSeat = await transaction.Aggregate<Seat>(fromSeatId) ??
            throw new SeatNotFoundException { SeatId = fromSeatId };

        var toSeat = await transaction.Aggregate<Seat>(toSeatId) ??
            throw new SeatNotFoundException { SeatId = toSeatId };

        toSeat.MoveReservationFor(fromSeat, initiator);

        transaction.Update(fromSeat);
        transaction.Update(toSeat);
    }
}