using Seatpicker.UserContext.Application.UserToken;

namespace Seatpicker.UserContext.Domain.Registration.Ports;

public interface IDiscordAccessTokenProvider
{
    Task<DiscordAccessToken> GetFor(string discordToken);
}
