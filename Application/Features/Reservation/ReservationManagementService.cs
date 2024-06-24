using Seatpicker.Domain;

namespace Seatpicker.Application.Features.Reservation;

public class ReservationManagementService
{
    private readonly IUserProvider _userProvider;
    private readonly IAggregateTransaction _transaction;
    private readonly IDocumentReader _reader;
    private readonly User _initiator;

    internal ReservationManagementService(IUserProvider userProvider,
        IAggregateTransaction transaction,
        IDocumentReader reader,
        User initiator)
    {
        _userProvider = userProvider;
        _transaction = transaction;
        _reader = reader;
        _initiator = initiator;
    }

    public async Task Create(string lanId, string seatId, string userId)
    {
        var userToReserveFor
            = await _userProvider.GetById(userId) ?? throw new UserNotFoundException { UserId = userId };

        var seatToReserve = await _transaction.Aggregate<Seat>(seatId) ??
            throw new SeatNotFoundException { SeatId = seatId };

        var numReservedSeatsByUser = _reader.Query<ProjectedSeat>()
            .Where(seat => seat.LanId == lanId)
            .Count(seat => seat.ReservedBy != null && seat.ReservedBy == userId);

        seatToReserve.MakeReservationFor(userToReserveFor, numReservedSeatsByUser, _initiator);

        _transaction.Update(seatToReserve);
    }

   public async Task Delete(string lanId, string seatId)
    {
        var seat = await _transaction.Aggregate<Seat>(seatId) ?? throw new SeatNotFoundException { SeatId = seatId };

        seat.RemoveReservationFor(_initiator);

        _transaction.Update(seat);
    }

   public async Task Move(string lanId, string fromSeatId, string toSeatId)
    {
        var fromSeat = await _transaction.Aggregate<Seat>(fromSeatId) ??
            throw new SeatNotFoundException { SeatId = fromSeatId };

        var toSeat = await _transaction.Aggregate<Seat>(toSeatId) ??
            throw new SeatNotFoundException { SeatId = toSeatId };

        toSeat.MoveReservationFor(fromSeat, _initiator);

        _transaction.Update(fromSeat);
        _transaction.Update(toSeat);
    }
}