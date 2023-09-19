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
    private readonly IAggregateTransaction transaction;

    public ReservationService(IAggregateTransaction transaction)
    {
        this.transaction = transaction;
    }

    public async Task Create(Guid seatId, User user)
    {
        var seatToReserve = await transaction.Aggregate<Seat>(seatId) ??
                            throw new SeatNotFoundException { SeatId = seatId };

        var seatsReservedByUser = transaction.Query<Seat>()
            .Where(seat => seat.ReservedBy != null && seat.ReservedBy== user.Id)
            .ToImmutableList();

        seatToReserve.MakeReservation(user, seatsReservedByUser);

        transaction.Update(seatToReserve);
    }

    public async Task Remove(Guid seatId, User user)
    {
        var seat = await transaction.Aggregate<Seat>(seatId) ?? throw new SeatNotFoundException { SeatId = seatId };

        seat.RemoveReservation(user);

        transaction.Update(seat);
    }

    public async Task Move(Guid fromSeatId, Guid toSeatId, User user)
    {
        var fromSeat = await transaction.Aggregate<Seat>(fromSeatId) ??
                       throw new SeatNotFoundException { SeatId = fromSeatId };

        var toSeat = await transaction.Aggregate<Seat>(toSeatId) ??
                     throw new SeatNotFoundException { SeatId = toSeatId };

        toSeat.MoveReservation(user, fromSeat);

        transaction.Update(fromSeat);
        transaction.Update(toSeat);
    }
}