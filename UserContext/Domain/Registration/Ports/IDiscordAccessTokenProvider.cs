using Seatpicker.Domain.Application.UserToken;

namespace Seatpicker.Domain.Domain.Registration.Ports;

public interface IDiscordAccessTokenProvider
{
    Task<DiscordAccessToken> GetFor(string discordToken);
}
