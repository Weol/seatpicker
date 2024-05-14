using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

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

    public Task<(string Token, DateTimeOffset ExpiresAt)> CreateToken(AuthenticationToken authenticationToken)
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
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Name, authenticationToken.Nick),
            new(JwtRegisteredClaimNames.Sub, authenticationToken.Id),
        };

        if (authenticationToken.Avatar is not null) 
            defaultClaims.Add(new Claim("avatar", authenticationToken.Avatar));

        if (authenticationToken.GuildId is not null)
            defaultClaims.Add(new Claim(GuildIdClaimName, authenticationToken.GuildId));
        
        var roleClaims = authenticationToken.Roles
            .Select(role => new Claim(ClaimTypes.Role, role.ToString()));

        var handler = new JwtSecurityTokenHandler();

        var now = DateTime.UtcNow;
        var expiresAt = now.AddSeconds(options.TokenLifetime);

        var token = handler.CreateJwtSecurityToken(
            authenticationToken.GuildId,
            authenticationToken.Id,
            new ClaimsIdentity(roleClaims.Concat(defaultClaims)),
            now.AddSeconds(-1),
            expiresAt,
            now,
            new SigningCredentials(rsaSecurityKey, SecurityAlgorithms.RsaSha256));

        return Task.FromResult((handler.WriteToken(token), new DateTimeOffset(expiresAt)));
    }
}