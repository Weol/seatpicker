using Seatpicker.Domain;

namespace Seatpicker.Application.Features.Login.Ports;

public interface IInviteDiscordUser
{
    Task Invite(User discordUser, string accessToken);
}
