using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using Seatpicker.Application.Features.Reservation.EventHandlers;
using Seatpicker.Domain;

namespace Seatpicker.Infrastructure.Adapters;

public class DiscordClient : IDiscordClient
{
    private readonly DiscordSocketClient discordSocketClient;
    private readonly IGuild guildId;

    public DiscordClient(DiscordSocketClient discordSocketClient, IOptions<DiscordClientOptions> options)
    {
        this.discordSocketClient = discordSocketClient;
        guildId = options.Value.GuildId;
    }

    private async Task<IGuild> GetGuild()
    {
        return await discordSocketClient.Guilds.First(guild => guild.Id )
    }

    private async Task<IGuildChannel> GetChannelByName(string name)
    {
        var guild = await GetGuild();

        var channels = await guild.GetChannelsAsync();
        foreach (var channel in channels)
        {
            if (channel.Name == name)
            {
                return channel;
            }
        }

        throw new Exception("Channel not found with name: " + name);
    }

    public async Task<string> GetInviteLink(User user)
    {
        var channel = await GetChannelByName("chat");
        var invite = new RestInvite(discordSocketClient, await GetGuild(), channel, "asdasd");
    }
}
