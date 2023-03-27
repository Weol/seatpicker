using Seatpicker.Domain;

namespace Seatpicker.Application.Features.Login.Ports;

public interface IDiscordLookupUser
{
    Task<User> Lookup(string accessToken);
}
