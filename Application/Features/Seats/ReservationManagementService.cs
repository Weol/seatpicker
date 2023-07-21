using Seatpicker.Domain;

namespace Seatpicker.Application.Features.Seats;

public interface IReservationManagement
{
    public Task MoveReservation(Guid fromSeatId, Guid toSeatId, User initiator);

    public Task DeleteReservation(Guid fromSeatId, Guid toSeatId, User initiator);
}

public class ReservationManagement : IReservationManagement
{
    public Task MoveReservation(Guid fromSeatId, Guid toSeatId, User initiator) => throw new NotImplementedException();

    public Task DeleteReservation(Guid fromSeatId, Guid toSeatId, User initiator) => throw new NotImplementedException();
}