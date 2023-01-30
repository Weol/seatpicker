namespace Seatpicker.Application.Features.Login.Ports;

public interface IDiscordAccessTokenProvider
{
    Task<string> GetFor(string discordToken);
}
