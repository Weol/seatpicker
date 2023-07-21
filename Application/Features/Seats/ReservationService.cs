using Seatpicker.Domain;

namespace Seatpicker.Application.Features.Seats;

public interface IReservationService
{
    public Task CreateReservation(Guid seatId, User user);

    public Task DeleteReservation(Guid seatId, User user);

    public Task MoveReservation(Guid seatId, User user);
}

public class ReservationService : IReservationService
{
    public Task CreateReservation(Guid seatId, User user) => throw new NotImplementedException();

    public Task DeleteReservation(Guid seatId, User user) => throw new NotImplementedException();

    public Task MoveReservation(Guid seatId, User user) => throw new NotImplementedException();
}