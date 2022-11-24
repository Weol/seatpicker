namespace Seatpicker.Domain.Application.UserToken.Ports;

public interface IDiscordAccessTokenProvider
{
    Task<DiscordAccessToken> GetFor(string discordToken);
}
