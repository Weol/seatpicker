namespace Seatpicker.Domain.Application.UserToken.Ports;

public interface IDiscordLookupUser
{
    Task<DiscordUser> Lookup(string accessToken);
}
