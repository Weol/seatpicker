using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Seatpicker.Application.Features.Token.Ports;

namespace Seatpicker.Infrastructure;

public static class AuthExtensions
{
    public static IServiceCollection AddAuth(this IServiceCollection services)
    {
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .PostConfigure<IAuthCertificateProvider>(ConfigureJwtBearerOptions);

        services
            .AddAuthorization()
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        return services;
    }

    private static void ConfigureJwtBearerOptions(JwtBearerOptions options, IAuthCertificateProvider provider)
    {
        var certificate = provider.Get().GetAwaiter().GetResult();
        var key = new X509SecurityKey(certificate);

        options.TokenValidationParameters.IssuerSigningKey = key;
        options.TokenValidationParameters.ValidateAudience = false;
        options.TokenValidationParameters.ValidIssuer = certificate.Thumbprint;
    }
}