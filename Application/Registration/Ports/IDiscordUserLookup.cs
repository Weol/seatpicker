namespace Seatpicker.SeatContext.Registration.Ports;

public interface IDiscordLookupUser
{
    Task<DiscordUser> Lookup(string accessToken);
}
