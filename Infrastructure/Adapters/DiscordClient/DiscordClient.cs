using Discord;
using Discord.Rest;
using Microsoft.Extensions.Options;
using Seatpicker.Application.Features.Login;
using Seatpicker.Application.Features.Reservation.EventHandlers;
using Seatpicker.Domain;

namespace Seatpicker.Infrastructure.Adapters;

public class DiscordClient : IInviteDiscordUser
{
    private readonly DiscordRestClient discordRestClient;

    public DiscordClient(IOptions<DiscordClientOptions> options)
    {
        var client = new DiscordRestClient();

        client.LoginAsync(TokenType.Bot, options.Value.Token)
            .GetAwaiter()
            .GetResult();

        discordRestClient = client;
    }

    public async Task Invite(User user)
    {
        var guild = await discordRestClient.GetGuildsAsync();
    }
}
