using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;
using Bogus;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Seatpicker.Infrastructure.Adapters.Discord;

namespace Seatpicker.IntegrationTests.TestAdapters;

public class TestDiscordAdapter : DiscordAdapter
{
    private readonly IDictionary<string, TestUser> users = new ConcurrentDictionary<string, TestUser>();
    private readonly IDictionary<string, TestGuild> guilds = new ConcurrentDictionary<string, TestGuild>();

    public TestDiscordAdapter(
        HttpClient httpClient,
        IOptions<DiscordAdapterOptions> options,
        JsonSerializerOptions jsonSerializerOptions,
        ILogger<DiscordAdapter> logger,
        IMemoryCache memoryCache) : base(httpClient, options, jsonSerializerOptions, logger, memoryCache)
    {
    }

    public void AddGuild(string id, string name, string icon, IEnumerable<string>? guildRoleIds = null)
    {
        var discordGuild = new DiscordGuild(id, name, icon);

        var roles = guildRoleIds == null
            ? Array.Empty<DiscordGuildRole>()
            : guildRoleIds.Select(
                roleId => new DiscordGuildRole(
                    roleId,
                    new Faker().Random.Word(),
                    new Faker().Random.Int(10000, 99999),
                    null));

        guilds.Add(discordGuild.Id, new TestGuild(discordGuild, roles));
    }

    public (string DiscordToken, string RefreshToken) AddUser(
        DiscordUser discordUser,
        string guildId,
        string? guildNick = null,
        string? guildAvatar = null,
        params string[] guildRoleIds)
    {
        var guild = guilds[guildId];
        foreach (var guildRoleId in guildRoleIds)
        {
            if (guild.Roles.All(guildRole => guildRole.Id != guildRoleId))
                throw new Exception("Guild member cannot have a role that the guild is not set up with!");
        }

        var user = new TestUser(
            discordUser,
            new Faker().Random.AlphaNumeric(15),
            new Faker().Random.AlphaNumeric(15),
            new Faker().Random.AlphaNumeric(15),
            new Membership(guild, guildNick, guildAvatar, guildRoleIds));

        users.Add(user.DiscordUser.Id, user);

        return (user.DiscordToken, user.RefreshToken);
    }

    public (string DiscordToken, string RefreshToken) AddUser(DiscordUser discordUser)
    {
        var user = new TestUser(
            discordUser,
            new Faker().Random.AlphaNumeric(15),
            new Faker().Random.AlphaNumeric(15),
            new Faker().Random.AlphaNumeric(15),
            null);
        users.Add(discordUser.Id, user);

        return (user.DiscordToken, user.RefreshToken);
    }

    public override Task<DiscordAccessToken> GetAccessToken(string discordToken, string redirectUrl)
    {
        var matchingUser = users.Values.FirstOrDefault(x => x.DiscordToken == discordToken);

        if (matchingUser is null)
            throw new DiscordException("Error from Discord test adapter")
            {
                StatusCode = HttpStatusCode.NotFound,
                Body = "User not found"
            };

        return Task.FromResult(new DiscordAccessToken(matchingUser.AccessToken, 24000, matchingUser.RefreshToken));
    }

    public override Task<DiscordAccessToken> RefreshAccessToken(string refreshToken)
    {
        var matchingUser = users.Values.FirstOrDefault(x => x.RefreshToken == refreshToken);

        if (matchingUser is null)
            throw new DiscordException("Error from Discord test adapter")
            {
                StatusCode = HttpStatusCode.BadRequest,
                Body = "Refresh token not valid for some reason"
            };

        users.Remove(matchingUser.DiscordUser.Id);
        var newUser = matchingUser with
        {
            AccessToken = new Faker().Random.AlphaNumeric(15),
            RefreshToken = new Faker().Random.AlphaNumeric(15),
        };
        users[newUser.DiscordUser.Id] = newUser;

        return Task.FromResult(new DiscordAccessToken(newUser.AccessToken, 24000, newUser.RefreshToken));
    }

    public override Task<DiscordUser> Lookup(string accessToken)
    {
        var matchingUser = users.Values.First(x => x.AccessToken == accessToken);

        return Task.FromResult(matchingUser.DiscordUser);
    }

    public override Task<IEnumerable<DiscordGuild>> GetGuilds()
    {
        return Task.FromResult(guilds.Values.Select(guild => guild.DiscordGuild));
    }

    public override Task<IEnumerable<DiscordGuildRole>> GetGuildRoles(string guildId)
    {
        var guild = guilds.Values.First(x => x.DiscordGuild.Id == guildId);
        return Task.FromResult(guild.Roles);
    }

    public override Task<DiscordGuildMember?> GetGuildMember(string guildId, string memberId)
    {
        var matchingUser = users.Values.FirstOrDefault(
            x => x.DiscordUser.Id == memberId && x.Membership != null && x.Membership.Guild.DiscordGuild.Id == guildId);

        if (matchingUser is null) return Task.FromResult<DiscordGuildMember?>(null);

        return Task.FromResult<DiscordGuildMember?>(
            new DiscordGuildMember(
                matchingUser.DiscordUser,
                matchingUser.Membership!.GuildAvatar,
                matchingUser.Membership!.GuildNick,
                matchingUser.Membership!.GuildRoleIds));
    }

    public override Task AddGuildMember(string guildId, string memberId, string accessToken)
    {
        return Task.CompletedTask;
    }

    private record TestUser(
        DiscordUser DiscordUser,
        string DiscordToken,
        string AccessToken,
        string RefreshToken,
        Membership? Membership);

    private record Membership(TestGuild Guild, string? GuildNick, string? GuildAvatar, string[] GuildRoleIds);

    private record TestGuild(DiscordGuild DiscordGuild, IEnumerable<DiscordGuildRole> Roles);
}