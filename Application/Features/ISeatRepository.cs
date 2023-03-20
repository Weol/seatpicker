using Seatpicker.Domain;

namespace Seatpicker.Application.Features;

public interface ISeatRepository
{
    Task<ICollection<Seat>> GetAll();

    Task<Seat?> Get(Guid seatId);

    Task<Seat?> GetByUser(string userId);

    Task Store(Seat seat);
}