using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Seatpicker.Domain.Application.UserToken.Ports;
using Seatpicker.Domain.Domain;
using Seatpicker.Domain.Domain.Registration.Ports;

namespace Seatpicker.Domain.Application.UserToken;

public interface IUserTokenService
{
    public Task<string> GetJwtFor(User user);
}

internal class UserTokenService : IUserTokenService
{
    private ILogger<UserTokenService> logger;
    private readonly Options options;
    private readonly IAuthCertificateProvider certificateProvider;
    private readonly ILanIdentityProvider lanIdentityProvider;

    public UserTokenService(ILogger<UserTokenService> logger, IOptions<Options> options,
        IAuthCertificateProvider certificateProvider,
        ILanIdentityProvider lanIdentityProvider, ILookupUser lookupUser,
        IDiscordAccessTokenProvider discordAccessTokenProvider)
    {
        this.logger = logger;
        this.options = options.Value;
        this.certificateProvider = certificateProvider;
        this.lanIdentityProvider = lanIdentityProvider;
    }

    public async Task<string> GetJwtFor(User user)
    {
        var certificate = await certificateProvider.Get();

        logger.LogDebug("Using auth certificate with thumbprint {Thumbprint}", certificate.Thumbprint);

        using var rsa = certificate.GetRSAPrivateKey();
        var rsaSecurityKey = new RsaSecurityKey(rsa);
        
        var userClaims = CreateClaimsForUser(user);

        var lanId = await lanIdentityProvider.GetCurrentLanId();
        
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, options.ClientId),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("lan_id", lanId.ToString()),
        };

        var now = DateTime.UtcNow;
        var token = new JwtSecurityToken
        (
            options.ClientId,
            user.Id,
            claims.Concat(userClaims),
            now.AddMilliseconds(-30),
            now.AddDays(30),
            new SigningCredentials(rsaSecurityKey, SecurityAlgorithms.RsaSha256)
        );

        return token.ToString();
    }

    private static IEnumerable<Claim> CreateClaimsForUser(User user)
    {
        var unixTimeCreated = user.CreatedAt.ToUniversalTime().ToUnixTimeMilliseconds().ToString();
        
        return new[]
        {
            new Claim("spu_id", user.Id),
            new Claim("spu_nick", user.Nick),
            new Claim("spu_avatar", user.Avatar),
            new Claim("spu_created_at_utc", unixTimeCreated),
            new Claim("spu_roles", string.Join(",", user.Roles)),
        };
    }

    public class Options
    {
        public string ClientId { get; set; } = null!;
    }
}

internal static class UserTokenServiceExtensions
{
    public static IServiceCollection AddUserTokenService(this IServiceCollection services,
        Action<UserTokenService.Options> configureAction)
    {
        services.Configure(configureAction);

        return services.AddScoped<IUserTokenService, UserTokenService>();
    }
}