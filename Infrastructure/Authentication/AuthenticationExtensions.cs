using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Seatpicker.Infrastructure.Authentication.Discord;

namespace Seatpicker.Infrastructure.Authentication;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddSeatpickerAuthentication(
        this IServiceCollection services)
    {
        services
            .AddSingleton<AuthenticationService>()
            .AddSingleton<JwtTokenCreator>()
            .AddUserManager()
            .AddDiscordAuthentication(ConfigureDiscordAuthentication)
            .AddAuthorization()
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services
            .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .PostConfigure<IOptions<AuthenticationOptions>>(ConfigureJwtBearerOptions);

        services.AddOptions<AuthenticationOptions>()
            .PostConfigure<ILogger<AuthenticationOptions>>(ConfigureDevelopmentCertificate);

        return services;
    }

    public static IApplicationBuilder UseSeatpickerAuthentication(this IApplicationBuilder app)
    {
        return app
            .UseAuthentication()
            .UseAuthorization();
    }

    private static void ConfigureJwtBearerOptions(
        JwtBearerOptions options,
        IOptions<AuthenticationOptions> discordOptions)
    {
        var securityKey = new X509SecurityKey(discordOptions.Value.SigningCertificate);

        options.TokenValidationParameters.IssuerSigningKey = securityKey;
        options.TokenValidationParameters.ValidateIssuer = false;
        options.TokenValidationParameters.ValidateAudience = false;
        options.TokenValidationParameters.NameClaimType = JwtRegisteredClaimNames.Name;
        options.TokenValidationParameters.RoleClaimType = ClaimTypes.Role;
    }

    private static void ConfigureDiscordAuthentication(
        AuthenticationOptions options,
        IConfiguration configuration)
    {
        configuration.GetSection("Authentication").Bind(options);

        // Configuration values from key vault
        options.Base64SigningCertificate = configuration["SigningCertificate"];
    }

    private static void ConfigureDevelopmentCertificate(AuthenticationOptions options, ILogger<AuthenticationOptions> logger)
    {
        if (options.Base64SigningCertificate is not null) return;
        
        logger.LogWarning("No signing certificate was provided, generating certificate for development purposes");

        var rsa = RSA.Create();
        var req = new CertificateRequest("cn=test", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        var certificate = req.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(5));
        options.Base64SigningCertificate = Convert.ToBase64String(certificate.Export(X509ContentType.Pfx, ""));
    }
}