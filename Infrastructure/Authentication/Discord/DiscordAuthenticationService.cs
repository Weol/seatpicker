using Microsoft.Extensions.Options;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Adapters.Database.GuildRoleMapping;
using Seatpicker.Infrastructure.Authentication.Discord.DiscordClient;

namespace Seatpicker.Infrastructure.Authentication.Discord;

public class DiscordAuthenticationService
{
    private readonly DiscordClient.DiscordClient discordClient;
    private readonly DiscordJwtTokenCreator tokenCreator;
    private readonly DiscordAuthenticationOptions options;
    private readonly UserManager userManager;
    private readonly GuildRoleMappingRepository roleMappingRepository;

    public DiscordAuthenticationService(
        DiscordClient.DiscordClient discordClient,
        DiscordJwtTokenCreator tokenCreator,
        IOptions<DiscordAuthenticationOptions> options,
        UserManager userManager,
        GuildRoleMappingRepository roleMappingRepository)
    {
        this.discordClient = discordClient;
        this.tokenCreator = tokenCreator;
        this.userManager = userManager;
        this.roleMappingRepository = roleMappingRepository;
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
                await userManager.Store(
                    new User(user.Id, guildMember.Nick ?? guildMember.DiscordUser.Username,
                        guildMember.Avatar ?? guildMember.DiscordUser.Avatar), guildId);
            }

            await Task.Delay(500); // To make sure we dont hit the discord rate limiter
        }
    }

    public async Task<(string JwtToken, DateTimeOffset ExpiresAt, DiscordToken DiscordToken)> Renew(
        string refreshToken,
        string guildId)
    {
        var accessToken = await discordClient.RefreshAccessToken(refreshToken);
        var discordUser = await discordClient.Lookup(accessToken.AccessToken);

        return await CreateToken(accessToken, discordUser, guildId);
    }

    public async Task<(string JwtToken, DateTimeOffset ExpiresAt, DiscordToken DiscordToken)> Login(
        string discordToken,
        string guildId,
        string redirectUrl)
    {
        var accessToken = await discordClient.GetAccessToken(discordToken, redirectUrl);
        var discordUser = await discordClient.Lookup(accessToken.AccessToken);

        await discordClient.AddGuildMember(guildId, discordUser.Id, accessToken.AccessToken);

        return await CreateToken(accessToken, discordUser, guildId);
    }

    private async Task<(string JwtToken, DateTimeOffset ExpiresAt, DiscordToken DiscordToken)> CreateToken(
        DiscordAccessToken accessToken,
        DiscordUser discordUser,
        string guildId)
    {
        var (roles, username, avatar) = await GetUserInfo(discordUser, guildId);

        var isSuperadmin = roles.Any(role => role == Role.Superadmin);
        
        var token = new DiscordToken(
            discordUser.Id,
            username,
            avatar,
            accessToken.RefreshToken,
            roles,
            isSuperadmin ? null : guildId);

        var (jwtToken, expiresAt) = await tokenCreator.CreateToken(token, roles);
        await userManager.Store(new User(new UserId(discordUser.Id), username, avatar), guildId);

        return (jwtToken, expiresAt, token);
    }

    private async Task<(Role[] Roles, string Username, string? Avatar)> GetUserInfo(DiscordUser discordUser,
        string guildId)
    {
        var guildMember = await discordClient.GetGuildMember(guildId, discordUser.Id);

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
        GuildMember guildMember)
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
}