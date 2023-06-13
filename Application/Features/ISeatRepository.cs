using Seatpicker.Domain;
using Shared;

namespace Seatpicker.Application.Features;

public interface ISeatRepository : IRepository<Seat>
{
    Task<ICollection<Seat>> GetAll();

    Task<Seat?> Get(Guid seatId);

    Task<Seat?> GetByUser(string userId);
}