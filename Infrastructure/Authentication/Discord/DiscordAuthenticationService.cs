using Microsoft.Extensions.Options;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Adapters.Database.GuildRoleMapping;
using Seatpicker.Infrastructure.Adapters.Discord;

namespace Seatpicker.Infrastructure.Authentication.Discord;

public class DiscordAuthenticationService
{
    private readonly DiscordAdapter discordAdapter;
    private readonly JwtTokenCreator tokenCreator;
    private readonly AuthenticationOptions options;
    private readonly UserManager userManager;
    private readonly GuildRoleMappingRepository roleMappingRepository;

    public DiscordAuthenticationService(
        DiscordAdapter discordAdapter,
        JwtTokenCreator tokenCreator,
        IOptions<AuthenticationOptions> options,
        UserManager userManager,
        GuildRoleMappingRepository roleMappingRepository)
    {
        this.discordAdapter = discordAdapter;
        this.tokenCreator = tokenCreator;
        this.userManager = userManager;
        this.roleMappingRepository = roleMappingRepository;
        this.options = options.Value;
    }

    public async Task<(string JwtToken, DateTimeOffset ExpiresAt, DiscordToken DiscordToken)> Renew(
        string refreshToken,
        string guildId)
    {
        var accessToken = await discordAdapter.RefreshAccessToken(refreshToken);
        var discordUser = await discordAdapter.Lookup(accessToken.AccessToken);

        return await CreateToken(accessToken, discordUser, guildId);
    }

    public async Task<(string JwtToken, DateTimeOffset ExpiresAt, DiscordToken DiscordToken)> Login(
        string discordToken,
        string guildId,
        string redirectUrl)
    {
        var accessToken = await discordAdapter.GetAccessToken(discordToken, redirectUrl);
        var discordUser = await discordAdapter.Lookup(accessToken.AccessToken);

        await discordAdapter.AddGuildMember(guildId, discordUser.Id, accessToken.AccessToken);

        return await CreateToken(accessToken, discordUser, guildId);
    }

    private async Task<(string JwtToken, DateTimeOffset ExpiresAt, DiscordToken DiscordToken)> CreateToken(
        DiscordAccessToken accessToken,
        DiscordUser discordUser,
        string guildId)
    {
        var (roles, username, avatar) = await GetUserInfo(discordUser, guildId);

        var token = new DiscordToken(
            discordUser.Id,
            username,
            avatar,
            accessToken.RefreshToken,
            roles,
            guildId,
            AuthenticationProvider.Discord);

        var (jwtToken, expiresAt) = await tokenCreator.CreateToken(token, roles);
        await userManager.Store(new User(discordUser.Id, username, avatar, roles));

        return (jwtToken, expiresAt, token);
    }

    private async Task<(Role[] Roles, string Username, string? Avatar)> GetUserInfo(DiscordUser discordUser,
        string guildId)
    {
        var guildMember = await discordAdapter.GetGuildMember(guildId, discordUser.Id);

        if (options.Superadmins.Any(admin => admin == discordUser.Id))
        {
            return (Enum.GetValues<Role>(), guildMember?.Nick ?? discordUser.Username,
                guildMember?.Avatar ?? discordUser.Avatar);
        }

        if (guildMember == null) return (new[] { Role.User }, discordUser.Username, discordUser.Avatar);

        var roleMappings = await roleMappingRepository.GetRoleMapping(guildId).ToArrayAsync();
        var roles = GetGuildMemberRoles(roleMappings, guildMember).Append(Role.User).Distinct().ToArray();

        return (roles, guildMember.Nick ?? discordUser.Username, guildMember.Avatar ?? discordUser.Avatar);
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