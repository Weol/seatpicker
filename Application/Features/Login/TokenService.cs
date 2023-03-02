using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Seatpicker.Domain;

namespace Seatpicker.Application.Features.Login;

public interface ITokenService
{
    public Task<string> CreateFor(User user, X509Certificate2 certificate, ICollection<Role> roles);
}

internal class TokenService : ITokenService
{
    private readonly ILogger<TokenService> logger;

    public TokenService(ILogger<TokenService> logger)
    {
        this.logger = logger;
    }

    public Task<string> CreateFor(User user, X509Certificate2 certificate, ICollection<Role> roles)
    {
        logger.LogInformation("Using certificate with thumbprint {Thumbprint} to create JWT", certificate.Thumbprint);

        using var rsa = certificate.GetRSAPrivateKey();
        var rsaSecurityKey = new RsaSecurityKey(rsa)
        {
            CryptoProviderFactory = new CryptoProviderFactory()
            {
                CacheSignatureProviders = false
            },
        };

        var defaultClaims = new System.Security.Claims.Claim[]
        {
            new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new ("spu_id", user.Id),
            new ("spu_nick", user.Nick),
            new ("spu_avatar", user.Avatar),
        };

        var roleClaims = roles.Select(role => new Claim("roles", role.ToString()));

        var handler = new JwtSecurityTokenHandler();

        var now = DateTime.UtcNow;
        var token = handler.CreateJwtSecurityToken(
            certificate.FriendlyName,
            user.Id,
            new ClaimsIdentity(roleClaims.Concat(defaultClaims)),
            now.AddMilliseconds(-30),
            now.AddDays(30),
            now,
            new SigningCredentials(rsaSecurityKey, SecurityAlgorithms.RsaSha256)
        );

        return Task.FromResult(handler.WriteToken(token));
    }
}