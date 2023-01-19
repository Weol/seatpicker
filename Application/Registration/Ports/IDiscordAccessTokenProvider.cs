namespace Seatpicker.SeatContext.Registration.Ports;

public interface IDiscordAccessTokenProvider
{
    Task<DiscordAccessToken> GetFor(string discordToken);
}
