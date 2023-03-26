using System.Text.Json;
using System.Text.Json.Serialization;
using MassTransit;
using Seatpicker.Domain;

namespace Seatpicker.Application.Features.Reservation.EventHandlers;

public interface IInviteDiscordUser
{
    Task Invite(User user);
}

public class InviteUserOnReservedEvent : IConsumer<SeatReservedEvent>
{
    private readonly IInviteDiscordUser inviteDiscordUser;

    public InviteUserOnReservedEvent(IInviteDiscordUser inviteDiscordUser)
    {
        this.inviteDiscordUser = inviteDiscordUser;
    }

    public async Task Consume(ConsumeContext<SeatReservedEvent> context)
    {
        await inviteDiscordUser.Invite(context.Message.User);
    }
}