using Microsoft.Extensions.Options;
using Seatpicker.Domain;

namespace Seatpicker.Infrastructure.Authentication;

public class AuthenticationService
{
    private readonly JwtTokenCreator tokenCreator;
    private readonly AuthenticationOptions options;
    private readonly UserManager userManager;

    public AuthenticationService(
        JwtTokenCreator tokenCreator,
        IOptions<AuthenticationOptions> options,
        UserManager userManager)
    {
        this.tokenCreator = tokenCreator;
        this.userManager = userManager;
        this.options = options.Value;
    }

    public async Task<(string JwtToken, DateTimeOffset ExpiresAt, AuthenticationToken AuthenticationToken)> Login(
        string providerUserId,
        AuthenticationProvider provider,
        string nick,
        string? avatar,
        string refreshToken,
        Role[] roles,
        string? guildId)
    {
        var token = AuthenticationToken.CreateWithProviderId(
            providerUserId,
            provider,
            nick,
            avatar,
            refreshToken,
            roles,
            guildId);

        var isSuperAdmin = IsSuperAdmin(token.Id);
        
        if (roles.Contains(Role.Superadmin) && !isSuperAdmin)
            throw new IllegalLoginAttemptException("Only whitelisted users can have the superadmin role");
        
        if (guildId is null && !isSuperAdmin) 
            throw new IllegalLoginAttemptException("Only superadmins can have a token with no guild id");

        if (isSuperAdmin) roles = Enum.GetValues<Role>();

        var (jwtToken, expiresAt) = await tokenCreator.CreateToken(token);
        await userManager.Store(guildId, new User(token.Id, nick, avatar, roles));

        return (jwtToken, expiresAt, token);
    }

    // Virtual for testing
    public virtual bool IsSuperAdmin(string userId)
    {
        return options.Superadmins.Any(admin => admin == userId);
    }

    public class IllegalLoginAttemptException : Exception
    {
        public IllegalLoginAttemptException(string? message) : base(message)
        {
        }
    } 
}