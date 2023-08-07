using Shared;

namespace Seatpicker.Application.Features.Token.Ports;

public interface IDiscordAccessTokenProvider : IPort
{
    Task<string> GetFor(string discordToken);
}
