using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Seatpicker.Infrastructure.Authentication.Discord;

public class DiscordJwtTokenCreator
{
    private readonly ILogger<DiscordJwtTokenCreator> logger;
    private readonly DiscordAuthenticationOptions options;

    public DiscordJwtTokenCreator(
        ILogger<DiscordJwtTokenCreator> logger,
        IOptions<DiscordAuthenticationOptions> options)
    {
        this.logger = logger;
        this.options = options.Value;
    }

    public Task<string> CreateToken(DiscordToken discordToken, ICollection<Role> roles)
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
            new ("refresh_token", discordToken.RefreshToken),
        };

        if (discordToken.GuildId is not null) defaultClaims.Add(new Claim("tenant_id", discordToken.GuildId));
        if (discordToken.Avatar is not null) defaultClaims.Add(new Claim("avatar", discordToken.Avatar));

        var roleClaims = roles.Select(role => new Claim(ClaimTypes.Role, role.ToString()));

        var handler = new JwtSecurityTokenHandler();

        var now = DateTime.UtcNow;
        var token = handler.CreateJwtSecurityToken(
            discordToken.GuildId,
            discordToken.Id,
            new ClaimsIdentity(roleClaims.Concat(defaultClaims)),
            now.AddMilliseconds(-30),
            now.AddSeconds(options.TokenLifetime),
            now,
            new SigningCredentials(rsaSecurityKey, SecurityAlgorithms.RsaSha256));

        return Task.FromResult(handler.WriteToken(token));
    }
}