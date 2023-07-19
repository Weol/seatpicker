using Shared;

namespace Seatpicker.Application.Features.Login.Ports;

public interface IDiscordAccessTokenProvider : IPort
{
    Task<string> GetFor(string discordToken);
}
