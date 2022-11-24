using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Seatpicker.Domain.Application.UserToken.Ports;
using Seatpicker.Domain.Domain;

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

        // Generating the token 
        var now = DateTime.UtcNow;

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, options.ClientId),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var lanId = await lanIdentityProvider.GetCurrentLanId();

        var token = new JwtSecurityToken
        (
            options.ClientId,
            lanId.ToString(),
            claims,
            now.AddMilliseconds(-30),
            now.AddDays(30),
            new SigningCredentials(rsaSecurityKey, SecurityAlgorithms.RsaSha256)
        );

        return token.ToString();
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

        return services.AddSingleton<IUserTokenService, UserTokenService>();
    }
}