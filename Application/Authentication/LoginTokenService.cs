using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Application.Authentication.Ports;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Application.Authentication;

public interface ILoginTokenService
{
    public Task<string> GetTokenFor(string id, string password);
}

internal class LoginTokenService : ILoginTokenService
{
    private readonly IAuthenticationCertificateProvider certificateProvider;

    public LoginTokenService(IAuthenticationCertificateProvider certificateProvider)
    {
        this.certificateProvider = certificateProvider;
    }

    public async Task<string> GetTokenFor(string id, string password)
    {
        var certificate = await certificateProvider.Get();

        using var rsa = certificate.GetRSAPrivateKey();
        var rsaSecurityKey = new RsaSecurityKey(rsa);

        // Generating the token 
        var now = DateTime.UtcNow;

        var claims = new[] {
            new Claim(JwtRegisteredClaimNames.Sub, "YOUR_CLIENTID"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var handler = new JwtSecurityTokenHandler();

        var token = new JwtSecurityToken
        (
            "YOUR_CLIENTID",
            "https://AAAS_PLATFORM/idp/YOUR_TENANT/authn/token",
            claims,
            now.AddMilliseconds(-30),
            now.AddMinutes(60),
            new SigningCredentials(rsaSecurityKey, SecurityAlgorithms.RsaSha256)
        );

        return token.ToString();
    }
}

internal static class LoginServiceExtensions
{
    public static IServiceCollection AddLoginService(this IServiceCollection services)
    {
        return services.AddSingleton<ILoginTokenService, LoginTokenService>();
    }
}
