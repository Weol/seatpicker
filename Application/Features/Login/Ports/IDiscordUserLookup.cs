namespace Seatpicker.Application.Features.Login.Ports;

public interface IDiscordLookupUser
{
    Task<DiscordUser> Lookup(string accessToken);
}
