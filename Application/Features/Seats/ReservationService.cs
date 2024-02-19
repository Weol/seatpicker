using Seatpicker.Domain;

namespace Seatpicker.Application.Features.Seats;

public interface IReservationService
{
    public Task Create(Guid lanId, Guid seatId, User user);

    public Task Remove(Guid lanId, Guid seatId, User user);

    public Task Move(Guid lanId, Guid fromSeatId, Guid toSeatId, User user);
}

public class ReservationService : IReservationService
{
    private readonly IAggregateRepository aggregateRepository;
    private readonly IDocumentRepository documentRepository;
    private readonly IReservationNotifier notifier;

    public ReservationService(IAggregateRepository aggregateRepository, IReservationNotifier notifier, IDocumentRepository documentRepository)
    {
        this.aggregateRepository = aggregateRepository;
        this.notifier = notifier;
        this.documentRepository = documentRepository;
    }

    public async Task Create(Guid lanId, Guid seatId, User user)
    {
        using var transaction = aggregateRepository.CreateTransaction();
        using var reader = documentRepository.CreateReader();

        var seatToReserve = await transaction.Aggregate<Seat>(seatId) ??
                            throw new SeatNotFoundException { SeatId = seatId };

        var numReservedSeatsByUser = reader.Query<ProjectedSeat>()
            .Where(seat => seat.LanId == lanId)
            .Count(seat => seat.ReservedBy != null && seat.ReservedBy == user.Id);

        seatToReserve.MakeReservation(user, numReservedSeatsByUser);

        transaction.Update(seatToReserve);
        await transaction.Commit();

        await notifier.NotifySeatReservationChanged(seatToReserve);
    }

    public async Task Remove(Guid lanId, Guid seatId, User user)
    {
        using var transaction = aggregateRepository.CreateTransaction();
        var seat = await transaction.Aggregate<Seat>(seatId) ?? throw new SeatNotFoundException { SeatId = seatId };

        seat.RemoveReservation(user);

        transaction.Update(seat);
        await transaction.Commit();

        await notifier.NotifySeatReservationChanged(seat);
    }

    public async Task Move(Guid lanId, Guid fromSeatId, Guid toSeatId, User user)
    {
        using var transaction = aggregateRepository.CreateTransaction();
        var fromSeat = await transaction.Aggregate<Seat>(fromSeatId) ??
                       throw new SeatNotFoundException { SeatId = fromSeatId };

        var toSeat = await transaction.Aggregate<Seat>(toSeatId) ??
                     throw new SeatNotFoundException { SeatId = toSeatId };

        toSeat.MoveReservation(user, fromSeat);

        transaction.Update(fromSeat);
        transaction.Update(toSeat);
        await transaction.Commit();

        await Task.WhenAll(
            notifier.NotifySeatReservationChanged(fromSeat),
            notifier.NotifySeatReservationChanged(toSeat));
    }
}