using Seatpicker.Domain;

namespace Seatpicker.Application.Features.Seats;

public interface IReservationService
{
    public Task Create(Guid seatId, User user);

    public Task Remove(Guid seatId, User user);

    public Task Move(Guid fromSeatId, Guid toSeatId, User user);
}

public class ReservationService : IReservationService
{
    private readonly IAggregateRepository aggregateRepository;

    public ReservationService(IAggregateRepository aggregateRepository)
    {
        this.aggregateRepository = aggregateRepository;
    }

    public async Task Create(Guid seatId, User user)
    {
        await using var transaction = aggregateRepository.CreateTransaction();

        var seatToReserve = await transaction.Aggregate<Seat>(seatId);
        if (seatToReserve is null) throw new SeatNotFoundException { SeatId = seatId };

        var alreadyReservedSeat = transaction.Query<Seat>()
            .SingleOrDefault(seat => seat.ReservedBy != null && seat.ReservedBy.Id == user.Id);

        if (alreadyReservedSeat is not null && alreadyReservedSeat.Id == seatToReserve.Id) return;
        if (alreadyReservedSeat is not null) throw new DuplicateSeatReservationException
        {
            ExistingSeatReservation = seatToReserve,
            AttemptedSeatReservation = alreadyReservedSeat,
            User = user,
        };

        seatToReserve.Reserve(user);

        transaction.Update(seatToReserve);

        transaction.Commit();
    }

    public async Task Remove(Guid seatId, User user)
    {
        await using var transaction = aggregateRepository.CreateTransaction();

        var seat = await transaction.Aggregate<Seat>(seatId);
        if (seat is null) throw new SeatNotFoundException { SeatId = seatId };

        seat.Unreserve(user);

        transaction.Update(seat);

        transaction.Commit();
    }

    public async Task Move(Guid fromSeatId, Guid toSeatId, User user)
    {
        await using var transaction = aggregateRepository.CreateTransaction();

        var fromSeat = await transaction.Aggregate<Seat>(fromSeatId);
        if (fromSeat is null) throw new SeatNotFoundException { SeatId = fromSeatId };

        var toSeat = await transaction.Aggregate<Seat>(toSeatId);
        if (toSeat is null) throw new SeatNotFoundException { SeatId = toSeatId };

        toSeat.MoveReservation(fromSeat, user);

        transaction.Update(fromSeat);
        transaction.Update(toSeat);

        transaction.Commit();
    }
}