using Seatpicker.Domain;

namespace Seatpicker.Infrastructure.Authentication;

public class AuthenticationToken
{
    public static AuthenticationToken CreateWithProviderId(string providerId,
        AuthenticationProvider provider,
        string nick,
        string? avatar,
        string refreshToken,
        Role[] roles,
        string? guildId
    )
    {
        return new AuthenticationToken(provider.Prefix + providerId,
            nick,
            avatar,
            refreshToken,
            roles,
            guildId);
    }
    
    private AuthenticationToken(string id,
        string nick,
        string? avatar,
        string refreshToken,
        Role[] roles,
        string? guildId)
    {
        Id = id;
        Nick = nick;
        Avatar = avatar;
        RefreshToken = refreshToken;
        Roles = roles;
        GuildId = guildId;
    }

    public string Id { get; }
    public string Nick { get; }
    public string? Avatar { get; }
    public string RefreshToken { get; }
    public Role[] Roles { get; }
    public string? GuildId { get; }
}