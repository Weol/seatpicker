using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Seatpicker.Domain;

namespace Seatpicker.Infrastructure.Authentication;

public class JwtTokenCreator
{
    public const string GuildIdClaimName = "guild_id";
    
    private readonly ILogger<JwtTokenCreator> logger;
    private readonly AuthenticationOptions options;

    public JwtTokenCreator(
        ILogger<JwtTokenCreator> logger,
        IOptions<AuthenticationOptions> options)
    {
        this.logger = logger;
        this.options = options.Value;
    }

    public Task<(string Token, DateTimeOffset ExpiresAt)> CreateToken(DiscordToken discordToken, ICollection<Role> roles)
    {
        var certificate = options.SigningCertificate;

        logger.LogDebug("Using certificate with thumbprint {Thumbprint} to create JWT", certificate.Thumbprint);

        using var rsa = certificate.GetRSAPrivateKey();
        var rsaSecurityKey = new RsaSecurityKey(rsa)
        {
            CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false, },
        };

        var defaultClaims = new List<Claim>
        {
            new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new (JwtRegisteredClaimNames.Name, discordToken.Nick),
            new (JwtRegisteredClaimNames.Sub, discordToken.Id),
            new (GuildIdClaimName, discordToken.GuildId),
        };

        if (discordToken.Avatar is not null) defaultClaims.Add(new Claim("avatar", discordToken.Avatar));

        var roleClaims = roles.Select(role => new Claim(ClaimTypes.Role, role.ToString()));

        var handler = new JwtSecurityTokenHandler();

        var now = DateTime.UtcNow;
        var expiresAt = now.AddSeconds(options.TokenLifetime);
        
        var token = handler.CreateJwtSecurityToken(
            discordToken.GuildId,
            discordToken.Id,
            new ClaimsIdentity(roleClaims.Concat(defaultClaims)),
            now.AddSeconds(-1),
            expiresAt,
            now,
            new SigningCredentials(rsaSecurityKey, SecurityAlgorithms.RsaSha256));

        return Task.FromResult((handler.WriteToken(token), new DateTimeOffset(expiresAt)));
    }
}