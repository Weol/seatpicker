﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Seatpicker.Domain;

namespace Seatpicker.Application.Features.Login;

public interface IJwtTokenService
{
    public Task<string> CreateFor(User user, X509Certificate2 certificate);
}

internal class JwtTokenService : IJwtTokenService
{
    private readonly ILogger<JwtTokenService> logger;

    public JwtTokenService(ILogger<JwtTokenService> logger)
    {
        this.logger = logger;
    }

    public Task<string> CreateFor(User user, X509Certificate2 certificate)
    {
        logger.LogInformation("Using certificate with thumbprint {Thumbprint} to create JWT", certificate.Thumbprint);

        using var rsa = certificate.GetRSAPrivateKey();
        var rsaSecurityKey = new RsaSecurityKey(rsa)
        {
            CryptoProviderFactory = new CryptoProviderFactory()
            {
                CacheSignatureProviders = false
            }
        };

        var userClaims = CreateClaimsForUser(user);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var handler = new JwtSecurityTokenHandler();

        var now = DateTime.UtcNow;
        var token = handler.CreateJwtSecurityToken(
            certificate.FriendlyName,
            user.Id,
            new ClaimsIdentity(claims.Concat(userClaims)),
            now.AddMilliseconds(-30),
            now.AddDays(30),
            now,
            new SigningCredentials(rsaSecurityKey, SecurityAlgorithms.RsaSha256)
        );

        return Task.FromResult(handler.WriteToken(token));
    }

    private static IEnumerable<Claim> CreateClaimsForUser(User user)
    {
        return new[]
        {
            new Claim("spu_id", user.Id),
            new Claim("spu_nick", user.Nick),
            new Claim("spu_avatar", user.Avatar),
        };
    }
}