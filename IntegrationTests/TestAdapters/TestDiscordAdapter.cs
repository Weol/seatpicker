using System.Net;
using System.Text.Json;
using Bogus;
using Discord.Rest;
using JasperFx.Core;
using Marten.Linq.SoftDeletes;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Seatpicker.Infrastructure.Adapters.Database.GuildRoleMapping;
using Seatpicker.Infrastructure.Adapters.Discord;

namespace Seatpicker.IntegrationTests.TestAdapters;

public class TestDiscordAdapter : DiscordAdapter
{
    private readonly ISet<TestUser> users = new HashSet<TestUser>();
    private readonly ISet<TestGuild> guilds = new HashSet<TestGuild>();

    public TestDiscordAdapter(HttpClient httpClient, IOptions<DiscordAdapterOptions> options, JsonSerializerOptions jsonSerializerOptions, ILogger<Infrastructure.Adapters.Discord.DiscordAdapter> logger, IMemoryCache memoryCache) : base(httpClient, options, jsonSerializerOptions, logger, memoryCache)
    {
    }
    
    public void AddGuild(string id, IEnumerable<string>? guildRoleIds = null)
    {
        var discordGuild = new DiscordGuild
        {
            Id = id,
            Name = new Faker().Internet.DomainWord(),
        };

        var roles = guildRoleIds == null
            ? Array.Empty<DiscordGuildRole>()
            : guildRoleIds.Select(role => new DiscordGuildRole
            {
                Id = new Faker().Random.Int(1).ToString(),
                Color = new Faker().Random.Int(10000, 99999),
                Name = new Faker().Random.Word()
            });

        guilds.Add(new TestGuild(discordGuild, roles));
    }

    public (string DiscordToken, string RefreshToken) AddUser(DiscordUser discordUser,
        string guildId,
        string? guildNick = null,
        string? guildAvatar = null,
        params string[] guildRoleIds)
    {
        var guild = guilds.First(guild => guild.DiscordGuild.Id == guildId);
        foreach (var guildRoleId in guildRoleIds)
        {
            if (guild.Roles.All(guildRole => guildRole.Id != guildRoleId))
                throw new Exception("Guild member cannot have a role that the guild is not set up with!");
        }

        var user = new TestUser(discordUser,
            new Faker().Random.AlphaNumeric(15),
            new Faker().Random.AlphaNumeric(15),
            new Faker().Random.AlphaNumeric(15),
            new Membership(
                guild,
                guildNick,
                guildAvatar,
                guildRoleIds)
        );

        users.Add(user);

        return (user.DiscordToken, user.RefreshToken);
    }

    public (string DiscordToken, string RefreshToken) AddUser(DiscordUser discordUser)
    {
        var user = new TestUser(discordUser,
            new Faker().Random.AlphaNumeric(15),
            new Faker().Random.AlphaNumeric(15),
            new Faker().Random.AlphaNumeric(15),
            null
        );

        return (user.DiscordToken, user.RefreshToken);
    }

    public override Task<DiscordAccessToken> GetAccessToken(string discordToken, string redirectUrl)
    {
        var matchingUser = users.FirstOrDefault(x => x.DiscordToken == discordToken);

        if (matchingUser is null)
            throw new DiscordException("Error from Discord test adapter")
            {
                StatusCode = HttpStatusCode.NotFound,
                Body = "User not found"
            };

        return Task.FromResult(new DiscordAccessToken
        {
            AccessToken = matchingUser.AccessToken,
            ExpiresIn = 24000,
            RefreshToken = matchingUser.RefreshToken
        });
    }

    public override Task<DiscordAccessToken> RefreshAccessToken(string refreshToken)
    {
        var matchingUser = users.FirstOrDefault(x => x.RefreshToken == refreshToken);

        if (matchingUser is null)
            throw new DiscordException("Error from Discord test adapter")
            {
                StatusCode = HttpStatusCode.BadRequest,
                Body = "Refresh token not valid for some reason"
            };

        users.Remove(matchingUser);
        var newUser = matchingUser with
        {
            AccessToken = new Faker().Random.AlphaNumeric(15),
            RefreshToken = new Faker().Random.AlphaNumeric(15),
        };
        users.Add(newUser);

        return Task.FromResult(new DiscordAccessToken
        {
            AccessToken = matchingUser.AccessToken,
            ExpiresIn = 24000,
            RefreshToken = matchingUser.RefreshToken
        });
    }

    public override Task<DiscordUser> Lookup(string accessToken)
    {
        var matchingUser = users.First(x => x.AccessToken == accessToken);

        return Task.FromResult(matchingUser.DiscordUser);
    }

    public override Task<IEnumerable<DiscordGuild>> GetGuilds()
    {
        return Task.FromResult(guilds.Select(guild => guild.DiscordGuild));
    }

    public override Task<IEnumerable<DiscordGuildRole>> GetGuildRoles(string guildId)
    {
        var guild = guilds.First(x => x.DiscordGuild.Id == guildId);
        return Task.FromResult(guild.Roles);
    }

    public override Task<DiscordGuildMember?> GetGuildMember(string guildId, string memberId)
    {
        var matchingUser = users.FirstOrDefault(x =>
            x.DiscordUser.Id == memberId && x.Membership != null && x.Membership.Guild.DiscordGuild.Id == guildId);

        if (matchingUser is null) return Task.FromResult<DiscordGuildMember?>(null);

        return Task.FromResult<DiscordGuildMember?>(new DiscordGuildMember
        {
            DiscordUser = matchingUser.DiscordUser,
            Avatar = matchingUser.Membership!.GuildAvatar,
            Nick = matchingUser.Membership!.GuildNick,
            Roles = matchingUser.Membership!.GuildRoleIds
        });
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

    private record Membership(
        TestGuild Guild,
        string? GuildNick,
        string? GuildAvatar,
        string[] GuildRoleIds);

    private record TestGuild(DiscordGuild DiscordGuild, IEnumerable<DiscordGuildRole> Roles);
}