using Seatpicker.Domain;

namespace Seatpicker.Application.Features.Seats;

public interface IUserProvider
{
    public Task<User?> GetById(UserId userId);
}