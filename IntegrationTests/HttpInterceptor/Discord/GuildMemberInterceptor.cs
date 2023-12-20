using System.Net;
using System.Net.Http.Headers;
using Seatpicker.Infrastructure.Authentication.Discord.DiscordClient;

namespace Seatpicker.IntegrationTests.HttpInterceptor.Discord;

public class GuildMemberInterceptor : IInterceptor
{
    private readonly DiscordUser discordUser;
    private readonly string? guildNick;
    private readonly string? guildAvatar;
    private readonly string[] guildRoles;
    private readonly bool isMemberOfGuild;

    public GuildMemberInterceptor(DiscordUser discordUser, params string[] guildRoles)
    {
        this.discordUser = discordUser;
        this.guildRoles = guildRoles;
        isMemberOfGuild = true;
    }
    
    public GuildMemberInterceptor(DiscordUser discordUser, bool isMemberOfGuild)
    {
        this.discordUser = discordUser;
        this.isMemberOfGuild = isMemberOfGuild;
        guildRoles = Array.Empty<string>();
    }
    
    public GuildMemberInterceptor(DiscordUser discordUser, string? guildNick, string? guildAvatar, params string[] guildRoles)
    {
        this.discordUser = discordUser;
        this.guildNick = guildNick;
        this.guildAvatar = guildAvatar;
        this.guildRoles = guildRoles;
        isMemberOfGuild = true;
    }

    public bool Match(string uri, HttpHeaders headers, HttpRequestMessage request)
    {
        return uri.EndsWith($"members/{discordUser.Id}");
    }

    public (object? Response, HttpStatusCode Code) Response(HttpRequestMessage requestMessage)
    {
        if (!isMemberOfGuild)
        {
            return (null, HttpStatusCode.NotFound);
        }
        
        var response = new
        {
            id = discordUser.Id,
            nick = guildNick ?? discordUser.Username,
            avatar = guildAvatar ?? discordUser.Avatar ?? null,
            roles = guildRoles,
            user = new
            {
                id = discordUser.Id,
                username = discordUser.Username,
                discriminator = "1337",
                avatar = discordUser.Avatar ?? null
            }
        };

        return (response, HttpStatusCode.OK);
    }
}