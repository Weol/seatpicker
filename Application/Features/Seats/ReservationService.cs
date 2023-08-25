using System.Collections.Immutable;
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

        var seatToReserve = await transaction.Aggregate<Seat>(seatId) ??
                            throw new SeatNotFoundException { SeatId = seatId };

        var seatsReservedByUser = transaction.Query<Seat>()
            .Where(seat => seat.ReservedBy != null && seat.ReservedBy.Id == user.Id)
            .ToImmutableList();

        ReservationPolicy.EnsureCanReserve(user, seatToReserve, seatsReservedByUser);


        transaction.Update(seatToReserve);

        transaction.Commit();
    }

    public async Task Remove(Guid seatId, User user)
    {
        await using var transaction = aggregateRepository.CreateTransaction();

        var seat = await transaction.Aggregate<Seat>(seatId) ?? throw new SeatNotFoundException { SeatId = seatId };

        ReservationPolicy.EnsureCanUnreserve(user, seatToReserve, seatsReservedByUser);
        seat.UnReserve(user);

        transaction.Update(seat);

        transaction.Commit();
    }

    public async Task Move(Guid fromSeatId, Guid toSeatId, User user)
    {
        await using var transaction = aggregateRepository.CreateTransaction();

        var fromSeat = await transaction.Aggregate<Seat>(fromSeatId) ??
                       throw new SeatNotFoundException { SeatId = fromSeatId };

        var toSeat = await transaction.Aggregate<Seat>(toSeatId) ??
                     throw new SeatNotFoundException { SeatId = toSeatId };

        toSeat.MoveReservation(fromSeat, user);

        transaction.Update(fromSeat);
        transaction.Update(toSeat);

        transaction.Commit();
    }
}