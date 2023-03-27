using Discord;
using Discord.Rest;
using Microsoft.Extensions.Options;
using Seatpicker.Application.Features.Reservation.EventHandlers;
using Seatpicker.Domain;

namespace Seatpicker.Infrastructure.Adapters;

public class DiscordClient : IInviteDiscordUser
{
    private readonly DiscordRestClient discordRestClient;
    private readonly string guildId;

    public DiscordClient(IOptions<DiscordClientOptions> options)
    {
        discordRestClient = CreateDiscordRestClient(options.Value.Token);
        guildId = options.Value.GuildId;
    }

    private async Task<RestGuild> GetGuild()
    {
        return await discordRestClient.GetGuildAsync(ulong.Parse(guildId));
    }

    public async Task Invite(User user)
    {
        var guild = GetGuild();
    }

    private static DiscordRestClient CreateDiscordRestClient(string token)
    {
        var client = new DiscordRestClient();

        client.LoginAsync(TokenType.Bot, token)
            .GetAwaiter()
            .GetResult();

        return client;
    }
}
