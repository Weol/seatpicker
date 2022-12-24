using Seatpicker.Domain.Application.UserToken;

namespace Seatpicker.Domain.Domain.Registration.Ports;

public interface IDiscordLookupUser
{
    Task<DiscordUser> Lookup(string accessToken);
}
