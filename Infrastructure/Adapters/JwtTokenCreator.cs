using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Tokens;
using Seatpicker.Application.Features.Token.Ports;
using Seatpicker.Domain;

namespace Seatpicker.Infrastructure.Adapters;

internal class JwtTokenCreator : IJwtTokenCreator
{
    private readonly ILogger<JwtTokenCreator> logger;

    public JwtTokenCreator(ILogger<JwtTokenCreator> logger)
    {
        this.logger = logger;
    }

    public Task<string> CreateFor(User user, X509Certificate2 certificate, ICollection<Role> roles)
    {
        logger.LogDebug("Using certificate with thumbprint {Thumbprint} to create JWT", certificate.Thumbprint);

        using var rsa = certificate.GetRSAPrivateKey();
        var rsaSecurityKey = new RsaSecurityKey(rsa)
        {
            CryptoProviderFactory = new CryptoProviderFactory
            {
                CacheSignatureProviders = false,
            },
        };

        var defaultClaims = new List<Claim>
        {
            new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new ("spu_id", user.Id),
            new ("spu_nick", user.Nick),
        };

        if (user.Avatar is not null) defaultClaims.Add(new Claim("spu_avatar", user.Avatar));

        var roleClaims = roles.Select(role => new Claim(ClaimTypes.Role, role.ToString()));

        var handler = new JwtSecurityTokenHandler();

        var now = DateTime.UtcNow;
        var token = handler.CreateJwtSecurityToken(
            certificate.Thumbprint,
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

internal static class JwtTokenCreatorExtensions
{
    internal static IServiceCollection AddJwtTokenCreator(this IServiceCollection services)
    {
        return services.AddSingleton<IJwtTokenCreator, JwtTokenCreator>();
    }
}
