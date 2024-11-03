using Seatpicker.Application.Features.Reservation;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Authentication;

namespace Seatpicker.Infrastructure.Adapters;

public class UserProvider(UserManager userManager, GuildIdProvider guildIdProvider) : IUserProvider
{
    public async Task<User?> GetById(string userId)
    {
        return await userManager.GetById(guildIdProvider.GuildId, userId);
    }
}