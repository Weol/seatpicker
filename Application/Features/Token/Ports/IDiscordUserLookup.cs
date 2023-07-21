namespace Seatpicker.Application.Features.Token.Ports;

public interface IDiscordLookupUser
{
    Task<DiscordUser> Lookup(string accessToken);
}
