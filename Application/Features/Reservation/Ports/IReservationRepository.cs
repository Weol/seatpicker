using Seatpicker.Domain;

namespace Seatpicker.Application.Features.Reservation.Ports;

public interface ISeatRepository
{
    Task<ICollection<Domain.Reservation>> GetAll();

    Task<Domain.Reservation?> Get(Guid seatId);

    Task Store(Domain.Reservation reservation);
}