using Seatpicker.Domain;

namespace Seatpicker.Application.Features.Reservation.Ports;

public interface ISeatRepository
{
    Task<ICollection<Seat>> GetAll();

    Task<Seat?> Get(Guid seatId);

    Task Store(Seat seat);
}