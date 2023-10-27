using System.Collections.Immutable;
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
    private readonly IAggregateTransaction transaction;
    private readonly IDocumentReader reader;

    public ReservationService(IAggregateTransaction transaction, IDocumentReader reader)
    {
        this.transaction = transaction;
        this.reader = reader;
    }

    public async Task Create(Guid lanId, Guid seatId, User user)
    {
        var seatToReserve = await transaction.Aggregate<Seat>(seatId) ??
                            throw new SeatNotFoundException { SeatId = seatId };

        var numReservedSeatsByUser = reader.Query<ProjectedSeat>()
            .Count(seat => seat.ReservedBy != null && seat.ReservedBy.Value == user.Id);

        seatToReserve.MakeReservation(user, numReservedSeatsByUser);

        transaction.Update(seatToReserve);
    }

    public async Task Remove(Guid lanId, Guid seatId, User user)
    {
        var seat = await transaction.Aggregate<Seat>(seatId) ?? throw new SeatNotFoundException { SeatId = seatId };

        seat.RemoveReservation(user);

        transaction.Update(seat);
    }

    public async Task Move(Guid lanId, Guid fromSeatId, Guid toSeatId, User user)
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