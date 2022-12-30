using Seatpicker.UserContext.Application.UserToken;

namespace Seatpicker.UserContext.Domain.Registration.Ports;

public interface IDiscordLookupUser
{
    Task<DiscordUser> Lookup(string accessToken);
}
