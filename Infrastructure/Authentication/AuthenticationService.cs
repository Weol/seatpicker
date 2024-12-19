using Microsoft.Extensions.Options;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Adapters.Database;

namespace Seatpicker.Infrastructure.Authentication;

public class AuthenticationService(
    JwtTokenCreator tokenCreator,
    IOptions<AuthenticationOptions> options,
    DocumentRepository documentRepository)
{
    private readonly AuthenticationOptions options = options.Value;

    public async Task<(string JwtToken, DateTimeOffset ExpiresAt, AuthenticationToken AuthenticationToken)> Login(
        string userId,
        string nick,
        string? avatar,
        string refreshToken,
        Role[] roles,
        string guildId)
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

        if (guildId is not null)
        {
            await using var transaction = documentRepository.CreateTransaction(guildId);
            transaction.Store(new UserDocument(token.Id, nick, avatar, roles));
            await transaction.Commit();
        }

        return (jwtToken, expiresAt, token);
    }

    private bool IsSuperAdmin(string userId)
    {
        return options.Superadmins.Any(admin => admin == userId);
    }

    public class IllegalLoginAttemptException(string? message) : Exception(message);
}