using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Application.Authentication.Ports;
using Application.Ports;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Seatpicker.Domain.Registration;

namespace Application.Authentication;

public interface ILoginJwtService
{
    public Task<string> GetJwtFor(string discordToken);
}

internal class LoginJwtService : ILoginJwtService
{
    private ILogger<LoginJwtService> logger;
    private readonly Options options;
    private readonly IAuthCertificateProvider certificateProvider;
    private readonly ILanIdentityProvider lanIdentityProvider;
    private readonly ILookupUser lookupUser;
    private readonly IRegistrationService registrationService;

    public LoginJwtService(ILogger<LoginJwtService> logger, IOptions<Options> options, IAuthCertificateProvider certificateProvider, IRegistrationService registrationService, ILanIdentityProvider lanIdentityProvider, ILookupUser lookupUser)
    {
        this.logger = logger;
        this.options = options.Value;
        this.certificateProvider = certificateProvider;
        this.registrationService = registrationService;
        this.lanIdentityProvider = lanIdentityProvider;
        this.lookupUser = lookupUser;
    }

    public async Task<string> GetJwtFor(string discordToken)
    {
        var token = await CreateToken();

        var user = lookupUser.Lookup(discordToken);
        
        logger.LogInformation("Created ");
        
        return token.ToString();
    }

    private async Task<JwtSecurityToken> CreateToken()
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
        return token;
    }

    public class Options
    {
        public string ClientId { get; set; } = null!;
    }
}

internal static class LoginJwtServiceExtensions
{
    public static IServiceCollection AddLoginJwtService(this IServiceCollection services, Action<LoginJwtService.Options> configureAction)
    {
        services.Configure(configureAction);
        
        return services.AddSingleton<ILoginJwtService, LoginJwtService>();
    }
}
