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

    public async Task<(string Token, DateTimeOffset ExpiresAt, string RefreshToken, DiscordUser DiscordUser, Role[] Roles)> Renew(string refreshToken, string? guildId)
    {
        var accessToken = await discordClient.RefreshAccessToken(refreshToken);
        var discordUser = await discordClient.Lookup(accessToken.AccessToken);

        return await CreateTokenRequest(accessToken, discordUser, guildId);
    }

    public async Task<(string Token, DateTimeOffset ExpiresAt, string RefreshToken, DiscordUser DiscordUser, Role[] Roles)> Login(string discordToken, string? guildId)
    {
        var accessToken = await discordClient.GetAccessToken(discordToken);
        var discordUser = await discordClient.Lookup(accessToken.AccessToken);

        return await CreateTokenRequest(accessToken, discordUser, guildId);
    }

    private async Task<(string Token, DateTimeOffset ExpiresAt, string, DiscordUser DiscordUser, Role[] Roles)> CreateTokenRequest(
        DiscordAccessToken accessToken,
        DiscordUser discordUser,
        string? guildId)
    {
        // Minus 10 just to make sure that the discord token expires a bit after the jwt token actually expires
        var expiresAt = DateTimeOffset.UtcNow.AddSeconds(accessToken.ExpiresIn - 10);

        Role[] roles;
        if (guildId is null)
        {
            if (options.Admins.Any(admin => admin == discordUser.Id))
                roles = Enum.GetValues<Role>();
            else
                throw new DiscordAuthenticationException("Only whitelisted users can request a token with no guild claim");
        }
        else
        {
            var guildMember = await discordClient.GetGuildMember(guildId, discordUser.Id);
            var roleMappings = await GetRoleMapping(guildId).ToArrayAsync();

            roles = GetGuildMemberRoles(roleMappings, guildMember).Distinct().ToArray();
        }

        var token = new DiscordToken(
            discordUser.Id,
            discordUser.Username,
            discordUser.Avatar,
            accessToken.RefreshToken,
            guildId,
            expiresAt);

        var jwtToken = await tokenCreator.CreateToken(token, roles);
        await userManager.Store(new User(new UserId(discordUser.Id), discordUser.Username, discordUser.Avatar));

        return (jwtToken, expiresAt, accessToken.RefreshToken, discordUser, roles);
    }

    private IEnumerable<Role> GetGuildMemberRoles((string RoleId, Role Role)[] roleMapping, GuildMember guildMember)
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
        await using var transaction = documentRepository.CreateTransaction();
        transaction.Store(new GuildRoleMapping(guildId, mappings));
        transaction.Commit();
    }

    public async IAsyncEnumerable<(string RoleId, Role Role)> GetRoleMapping(string guildId)
    {
        var reader = documentRepository.CreateReader();

        var roleMappings = await reader.Get<GuildRoleMapping>(guildId) ??
                           new GuildRoleMapping(guildId, Array.Empty<(string RoleId, Role Role)>());

        foreach (var roleMappingsMapping in roleMappings.Mappings)
        {
            yield return roleMappingsMapping;
        }
    }

    public record GuildRoleMapping(string GuildId, IEnumerable<(string RoleId, Role Role)> Mappings) : IDocument
    {
        public string Id => GuildId;
    }
}

public class DiscordAuthenticationException : Exception
{
    public DiscordAuthenticationException(string message) : base(message)
    {
    }
}