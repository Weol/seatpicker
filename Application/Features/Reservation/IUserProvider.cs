using Seatpicker.Domain;

namespace Seatpicker.Application.Features.Reservation;

public interface IUserProvider
{
    public Task<User?> GetById(string userId);
}