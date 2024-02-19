using Seatpicker.Domain;

namespace Seatpicker.Application.Features.Seats;

public interface IReservationManagementService
{
    public Task Move(Guid lanId, Guid fromSeatId, Guid toSeatId, User initiator);

    public Task Create(Guid lanId, Guid seatId, string userId, User reservedBy);

    public Task Delete(Guid lanId, Guid seatId, User removedBy);
}

public class ReservationManagementService : IReservationManagementService
{
    private readonly IAggregateRepository aggregateRepository;
    private readonly IDocumentRepository documentRepository;
    private readonly IUserProvider userProvider;
    private readonly IReservationNotifier notifier;

    public ReservationManagementService(IUserProvider userProvider,
        IAggregateRepository aggregateRepository,
        IReservationNotifier notifier,
        IDocumentRepository documentRepository)
    {
        this.userProvider = userProvider;
        this.aggregateRepository = aggregateRepository;
        this.notifier = notifier;
        this.documentRepository = documentRepository;
    }

    public async Task Create(Guid lanId, Guid seatId, string userId, User reservedBy)
    {
        using var transaction = aggregateRepository.CreateTransaction();
        using var reader = documentRepository.CreateReader();

        var userToReserveFor
            = await userProvider.GetById(userId) ?? throw new UserNotFoundException { UserId = userId };

        var seatToReserve = await transaction.Aggregate<Seat>(seatId) ??
            throw new SeatNotFoundException { SeatId = seatId };

        var numReservedSeatsByUser = reader.Query<ProjectedSeat>()
            .Where(seat => seat.LanId == lanId)
            .Count(seat => seat.ReservedBy != null && seat.ReservedBy == userId);

        seatToReserve.MakeReservationFor(userToReserveFor, numReservedSeatsByUser, reservedBy);

        transaction.Update(seatToReserve);
        await transaction.Commit();

        await notifier.NotifySeatReservationChanged(seatToReserve);
    }

    public async Task Delete(Guid lanId, Guid seatId, User removedBy)
    {
        using var transaction = aggregateRepository.CreateTransaction();
        var seat = await transaction.Aggregate<Seat>(seatId) ?? throw new SeatNotFoundException { SeatId = seatId };

        seat.RemoveReservationFor(removedBy);

        transaction.Update(seat);
        await transaction.Commit();

        await notifier.NotifySeatReservationChanged(seat);
    }

    public async Task Move(Guid lanId, Guid fromSeatId, Guid toSeatId, User movedBy)
    {
        using var transaction = aggregateRepository.CreateTransaction();

        var fromSeat = await transaction.Aggregate<Seat>(fromSeatId) ??
            throw new SeatNotFoundException { SeatId = fromSeatId };

        var toSeat = await transaction.Aggregate<Seat>(toSeatId) ??
            throw new SeatNotFoundException { SeatId = toSeatId };

        toSeat.MoveReservationFor(fromSeat, movedBy);

        transaction.Update(fromSeat);
        transaction.Update(toSeat);
        await transaction.Commit();

        await Task.WhenAll(
            notifier.NotifySeatReservationChanged(fromSeat),
            notifier.NotifySeatReservationChanged(toSeat));
    }
}