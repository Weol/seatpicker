using Microsoft.Extensions.Options;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Adapters.Database.GuildRoleMapping;
using Seatpicker.Infrastructure.Adapters.Discord;

namespace Seatpicker.Infrastructure.Authentication.Discord;

public class DiscordAuthenticationService
{
    private readonly DiscordAdapter discordAdapter;
    private readonly AuthenticationService authenticationService;
    private readonly GuildRoleMappingRepository roleMappingRepository;

    public DiscordAuthenticationService(
        DiscordAdapter discordAdapter,
        AuthenticationService authenticationService,
        IOptions<AuthenticationOptions> options,
        GuildRoleMappingRepository roleMappingRepository)
    {
        this.discordAdapter = discordAdapter;
        this.authenticationService = authenticationService;
        this.roleMappingRepository = roleMappingRepository;
    }

    public async Task<(string JwtToken, DateTimeOffset ExpiresAt, AuthenticationToken DiscordToken)> Renew(
        string refreshToken,
        string guildId)
    {
        var accessToken = await discordAdapter.RefreshAccessToken(refreshToken);
        var discordUser = await discordAdapter.Lookup(accessToken.AccessToken);

        return await CreateToken(accessToken, discordUser, guildId);
    }

    public async Task<(string JwtToken, DateTimeOffset ExpiresAt, AuthenticationToken DiscordToken)> Login(
        string discordToken,
        string? guildId,
        string redirectUrl)
    {
        var accessToken = await discordAdapter.GetAccessToken(discordToken, redirectUrl);
        var discordUser = await discordAdapter.Lookup(accessToken.AccessToken);

        if (guildId is not null)
            await discordAdapter.AddGuildMember(guildId, discordUser.Id, accessToken.AccessToken);

        return await CreateToken(accessToken, discordUser, guildId);
    }

    private async Task<(string JwtToken, DateTimeOffset ExpiresAt, AuthenticationToken AuthenticationToken)>
        CreateToken(
            DiscordAccessToken accessToken,
            DiscordUser discordUser,
            string? guildId)
    {
        if (guildId is null)
        {
            return await authenticationService.Login(
                discordUser.Id,
                AuthenticationProvider.Discord,
                discordUser.Username,
                discordUser.Avatar,
                accessToken.RefreshToken,
                new[] { Role.Superadmin },
                null);
        }

        var (roles, guildNick, guildAvatar) = await GetGuildUserInfo(discordUser, guildId);

        return await authenticationService.Login(
            discordUser.Id,
            AuthenticationProvider.Discord,
            guildNick ?? discordUser.Username,
            guildAvatar ?? discordUser.Avatar,
            accessToken.RefreshToken,
            roles,
            null);
    }

    private async Task<(Role[] Roles, string? Nick, string? Avatar)> GetGuildUserInfo(DiscordUser discordUser,
        string guildId)
    {
        var guildMember = await discordAdapter.GetGuildMember(guildId, discordUser.Id);

        if (guildMember == null) return (new[] { Role.User }, discordUser.Username, discordUser.Avatar);

        var roleMappings = await roleMappingRepository.GetRoleMapping(guildId).ToArrayAsync();
        var roles = GetGuildMemberRoles(roleMappings, guildMember).Append(Role.User).Distinct().ToArray();

        return (roles, guildMember.Nick, guildMember.Avatar);
    }

    private static IEnumerable<Role> GetGuildMemberRoles((string RoleId, Role Role)[] roleMapping,
        DiscordGuildMember discordGuildMember)
    {
        yield return Role.User;

        foreach (var guildRoleId in discordGuildMember.Roles)
        {
            foreach (var mapping in roleMapping)
            {
                if (guildRoleId != mapping.RoleId) continue;

                yield return mapping.Role;
            }
        }
    }
}