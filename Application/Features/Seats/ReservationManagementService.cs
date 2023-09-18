using System.Collections.Immutable;
using Seatpicker.Domain;

namespace Seatpicker.Application.Features.Seats;

public interface IReservationManagementService
{
    public Task Move(UserId userToMove, Guid fromSeatId, Guid toSeatId, User initiator);

    public Task Create(Guid seatId, UserId userId, User reservedBy);

    public Task Remove(Guid seatId, User removedBy);
}

public class ReservationManagementService : IReservationManagementService
{
    private readonly IAggregateTransaction transaction;
    private readonly IUserProvider userProvider;

    public ReservationManagementService(IAggregateTransaction transaction, IUserProvider userProvider)
    {
        this.transaction = transaction;
        this.userProvider = userProvider;
    }

    public async Task Create(Guid seatId, UserId userId, User reservedBy)
    {
        var userToReserveFor = await userProvider.GetById(userId) ?? throw new UserNotFoundException { UserId = userId };

        var seatToReserve = await transaction.Aggregate<Seat>(seatId) ??
                            throw new SeatNotFoundException { SeatId = seatId };

        var seatsReservedByUser = transaction.Query<Seat>()
            .Where(seat => seat.ReservedBy != null && seat.ReservedBy == userId)
            .ToImmutableList();

        seatToReserve.MakeReservationFor(userToReserveFor, seatsReservedByUser, reservedBy);

        transaction.Update(seatToReserve);
    }

    public async Task Remove(Guid seatId, User removedBy)
    {
        var seat = await transaction.Aggregate<Seat>(seatId) ?? throw new SeatNotFoundException { SeatId = seatId };

        seat.RemoveReservationFor(removedBy);

        transaction.Update(seat);
    }

    public async Task Move(UserId userToMove, Guid fromSeatId, Guid toSeatId, User movedBy)
    {
        var userToMoveFor = await userProvider.GetById(userToMove) ?? throw new UserNotFoundException { UserId = userToMove };

        var fromSeat = await transaction.Aggregate<Seat>(fromSeatId) ??
                       throw new SeatNotFoundException { SeatId = fromSeatId };

        var toSeat = await transaction.Aggregate<Seat>(toSeatId) ??
                     throw new SeatNotFoundException { SeatId = toSeatId };

        toSeat.MoveReservationFor(userToMoveFor, fromSeat, movedBy);

        transaction.Update(fromSeat);
        transaction.Update(toSeat);
    }
}