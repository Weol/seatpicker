using Microsoft.Extensions.Options;
using Seatpicker.Domain;

namespace Seatpicker.Infrastructure.Authentication;

public class AuthenticationService(
    JwtTokenCreator tokenCreator,
    IOptions<AuthenticationOptions> options,
    UserManager userManager)
{
    private readonly AuthenticationOptions options = options.Value;

    public async Task<(string JwtToken, DateTimeOffset ExpiresAt, AuthenticationToken AuthenticationToken)> Login(
        string userId,
        string nick,
        string? avatar,
        string refreshToken,
        Role[] roles,
        string? guildId)
    {
        var isSuperAdmin = IsSuperAdmin(userId);

        if (roles.Contains(Role.Superadmin) && !isSuperAdmin)
            throw new IllegalLoginAttemptException("Only whitelisted users can have the superadmin role");

        if (guildId is null && !isSuperAdmin)
            throw new IllegalLoginAttemptException("Only superadmins can have a token with no guild id");

        if (isSuperAdmin) roles = Enum.GetValues<Role>();

        var token = new AuthenticationToken(
            userId,
            nick,
            avatar,
            refreshToken,
            roles,
            guildId);

        var (jwtToken, expiresAt) = await tokenCreator.CreateToken(token);

        await userManager.Store(guildId, new User(token.Id, nick, avatar, roles));

        return (jwtToken, expiresAt, token);
    }

    private bool IsSuperAdmin(string userId)
    {
        return options.Superadmins.Any(admin => admin == userId);
    }

    public class IllegalLoginAttemptException(string? message) : Exception(message);
}