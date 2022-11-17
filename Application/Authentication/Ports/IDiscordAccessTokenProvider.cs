using Application.Discord;

namespace Application.Authentication.Ports;

public interface IDiscordAccessTokenProvider
{
    Task<DiscordAccessToken> GetFor(string discordToken);
}
