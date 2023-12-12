using Microsoft.Extensions.Options;
using Seatpicker.Application.Features;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Authentication.Discord.DiscordClient;
using Shared;

namespace Seatpicker.Infrastructure.Authentication.Discord;

public class DiscordAuthenticationService
{
    private readonly DiscordClient.DiscordClient discordClient;
    private readonly DiscordJwtTokenCreator tokenCreator;
    private readonly IDocumentRepository documentRepository;
    private readonly DiscordAuthenticationOptions options;
    private readonly UserManager userManager;

    public DiscordAuthenticationService(
        DiscordClient.DiscordClient discordClient,
        DiscordJwtTokenCreator tokenCreator,
        IDocumentRepository documentRepository,
        IOptions<DiscordAuthenticationOptions> options,
        UserManager userManager)
    {
        this.discordClient = discordClient;
        this.tokenCreator = tokenCreator;
        this.documentRepository = documentRepository;
        this.userManager = userManager;
        this.options = options.Value;
    }

    public async Task ReloadUserDataFromGuild(string guildId)
    {
        var users = await userManager.GetAllInGuild(guildId);
        foreach (var user in users)
        {
            var guildMember = await discordClient.GetGuildMember(guildId, user.Id);
            if (guildMember is not null)
            {
                await userManager.Store(new User(user.Id, guildMember.Nick ?? guildMember.DiscordUser.Username, guildMember.Avatar ?? guildMember.DiscordUser.Avatar), guildId);
            }

            await Task.Delay(500); // To make sure we dont hit the discord rate limiter
        }
    }
    
    public async
        Task<(string Token, DateTimeOffset ExpiresAt, string RefreshToken, DiscordUser DiscordUser, Role[] Roles)>
        Renew(string refreshToken, string guildId)
    {
        var accessToken = await discordClient.RefreshAccessToken(refreshToken);
        var discordUser = await discordClient.Lookup(accessToken.AccessToken);

        return await CreateTokenRequest(accessToken, discordUser, guildId);
    }

    public async
        Task<(string Token, DateTimeOffset ExpiresAt, string RefreshToken, DiscordUser DiscordUser, Role[] Roles)>
        Login(string discordToken, string guildId, string redirectUrl)
    {
        var accessToken = await discordClient.GetAccessToken(discordToken, redirectUrl);
        var discordUser = await discordClient.Lookup(accessToken.AccessToken);

        await discordClient.AddGuildMember(guildId, discordUser.Id, accessToken.AccessToken);

        return await CreateTokenRequest(accessToken, discordUser, guildId);
    }

    private async Task<(string Token, DateTimeOffset ExpiresAt, string, DiscordUser DiscordUser, Role[] Roles)>
        CreateTokenRequest(
            DiscordAccessToken accessToken,
            DiscordUser discordUser,
            string guildId)
    {
        var (roles, username, avatar) = await GetUserInfo(discordUser, guildId);

        var token = new DiscordToken(
            discordUser.Id,
            discordUser.Username,
            discordUser.Avatar,
            accessToken.RefreshToken,
            guildId);

        var (jwtToken, expiresAt) = await tokenCreator.CreateToken(token, roles);
        await userManager.Store(new User(new UserId(discordUser.Id), username, avatar), guildId);

        return (jwtToken, expiresAt, accessToken.RefreshToken, discordUser, roles);
    }

    private async Task<(Role[] Roles, string Username, string? Avatar)> GetUserInfo(DiscordUser discordUser,
        string guildId)
    {
        var guildMember = await discordClient.GetGuildMember(guildId, discordUser.Id);
        
        if (options.Admins.Any(admin => admin == discordUser.Id))
        {
            return (Enum.GetValues<Role>(), guildMember?.Nick ?? discordUser.Username, guildMember?.Avatar ?? discordUser.Avatar);
        }

        if (guildMember == null) return (new[] { Role.User }, discordUser.Username, discordUser.Avatar);
        
        var roleMappings = await GetRoleMapping(guildId).ToArrayAsync();
        var roles = GetGuildMemberRoles(roleMappings, guildMember).Distinct().ToArray();
        
        return (roles, guildMember.Nick ?? discordUser.Username, guildMember.Avatar ?? discordUser.Avatar);
    }

    private static IEnumerable<Role> GetGuildMemberRoles((string RoleId, Role Role)[] roleMapping, GuildMember guildMember)
    {
        yield return Role.User;

        foreach (var guildRoleId in guildMember.Roles)
        {
            foreach (var mapping in roleMapping)
            {
                if (guildRoleId != mapping.RoleId) continue;

                yield return mapping.Role;
            }
        }
    }

    public async Task SetRoleMapping(string guildId, IEnumerable<(string RoleId, Role Role)> mappings)
    {
        using var transaction = documentRepository.CreateTransaction();

        var transformed = mappings
            .Select(mapping => new GuildRoleMappingEntry(mapping.RoleId, mapping.Role))
            .ToArray();

        transaction.Store(new GuildRoleMapping(
            guildId,
            transformed));
        await transaction.Commit();
    }

    public async IAsyncEnumerable<(string RoleId, Role Role)> GetRoleMapping(string guildId)
    {
        using var reader = documentRepository.CreateReader();

        var roleMappings = await reader.Get<GuildRoleMapping>(guildId) ??
            new GuildRoleMapping(guildId, Array.Empty<GuildRoleMappingEntry>());

        foreach (var roleMappingsMapping in roleMappings.Mappings)
        {
            yield return (roleMappingsMapping.RoleId, roleMappingsMapping.Role);
        }
    }

    public record GuildRoleMapping(string GuildId, IEnumerable<GuildRoleMappingEntry> Mappings) : IDocument
    {
        public string Id => GuildId;
    }

    public record GuildRoleMappingEntry(string RoleId, Role Role);
}

public class DiscordAuthenticationException : Exception
{
    public DiscordAuthenticationException(string message) : base(message)
    {
    }
}
