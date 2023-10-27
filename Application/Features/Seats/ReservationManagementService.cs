using System.Collections.Immutable;
using Seatpicker.Domain;

namespace Seatpicker.Application.Features.Seats;

public interface IReservationManagementService
{
    public Task Move(Guid lanId, UserId userToMove, Guid fromSeatId, Guid toSeatId, User initiator);

    public Task Create(Guid lanId, Guid seatId, UserId userId, User reservedBy);

    public Task Delete(Guid lanId, Guid seatId, User removedBy);
}

public class ReservationManagementService : IReservationManagementService
{
    private readonly IAggregateTransaction transaction;
    private readonly IDocumentReader reader;
    private readonly IUserProvider userProvider;

    public ReservationManagementService(IAggregateTransaction transaction, IUserProvider userProvider, IDocumentReader reader)
    {
        this.transaction = transaction;
        this.userProvider = userProvider;
        this.reader = reader;
    }

    public async Task Create(Guid lanId, Guid seatId, UserId userId, User reservedBy)
    {
        var userToReserveFor = await userProvider.GetById(userId) ?? throw new UserNotFoundException { UserId = userId };

        var seatToReserve = await transaction.Aggregate<Seat>(seatId) ??
                            throw new SeatNotFoundException { SeatId = seatId };

        var numReservedSeatsByUser = reader.Query<ProjectedSeat>()
            .Count(seat => seat.ReservedBy != null && seat.ReservedBy.Value == userId);

        seatToReserve.MakeReservationFor(userToReserveFor, numReservedSeatsByUser, reservedBy);

        transaction.Update(seatToReserve);
    }

    public async Task Delete(Guid lanId, Guid seatId, User removedBy)
    {
        var seat = await transaction.Aggregate<Seat>(seatId) ?? throw new SeatNotFoundException { SeatId = seatId };

        seat.RemoveReservationFor(removedBy);

        transaction.Update(seat);
    }

    public async Task Move(Guid lanId, UserId userToMove, Guid fromSeatId, Guid toSeatId, User movedBy)
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