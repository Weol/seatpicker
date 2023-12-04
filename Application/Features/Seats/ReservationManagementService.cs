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
    private readonly IAggregateRepository repository;
    private readonly IDocumentReader reader;
    private readonly IUserProvider userProvider;

    public ReservationManagementService(IUserProvider userProvider, IDocumentReader reader, IAggregateRepository repository)
    {
        this.userProvider = userProvider;
        this.reader = reader;
        this.repository = repository;
    }

    public async Task Create(Guid lanId, Guid seatId, UserId userId, User reservedBy)
    {
        await using var transaction = repository.CreateTransaction();

        var userToReserveFor = await userProvider.GetById(userId) ?? throw new UserNotFoundException { UserId = userId };

        var seatToReserve = await transaction.Aggregate<Seat>(seatId) ??
                            throw new SeatNotFoundException { SeatId = seatId };

        var numReservedSeatsByUser = reader.Query<ProjectedSeat>()
            .Where(seat => seat.LanId == lanId)
            .Count(seat => seat.ReservedBy != null && seat.ReservedBy.Value == userId.Value);

        seatToReserve.MakeReservationFor(userToReserveFor, numReservedSeatsByUser, reservedBy);

        transaction.Update(seatToReserve);
        transaction.Commit();
    }

    public async Task Delete(Guid lanId, Guid seatId, User removedBy)
    {
        await using var transaction = repository.CreateTransaction();
        var seat = await transaction.Aggregate<Seat>(seatId) ?? throw new SeatNotFoundException { SeatId = seatId };

        seat.RemoveReservationFor(removedBy);

        transaction.Update(seat);
        transaction.Commit();
    }

    public async Task Move(Guid lanId, UserId userToMove, Guid fromSeatId, Guid toSeatId, User movedBy)
    {
        await using var transaction = repository.CreateTransaction();
        var userToMoveFor = await userProvider.GetById(userToMove) ?? throw new UserNotFoundException { UserId = userToMove };

        var fromSeat = await transaction.Aggregate<Seat>(fromSeatId) ??
                       throw new SeatNotFoundException { SeatId = fromSeatId };

        var toSeat = await transaction.Aggregate<Seat>(toSeatId) ??
                     throw new SeatNotFoundException { SeatId = toSeatId };

        toSeat.MoveReservationFor(userToMoveFor, fromSeat, movedBy);

        transaction.Update(fromSeat);
        transaction.Update(toSeat);
        transaction.Commit();
    }
}