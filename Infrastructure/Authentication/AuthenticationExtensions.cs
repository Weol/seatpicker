using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Seatpicker.Infrastructure.Authentication.Discord;
using Seatpicker.Infrastructure.Authentication.Discord.DiscordClient;

namespace Seatpicker.Infrastructure.Authentication;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddSeatpickerAuthentication(
        this IServiceCollection services)
    {
        services
            .AddDiscordLogin(ConfigureDiscordAuthentication, ConfigureDiscordClient)
            .AddUserManager()
            .AddAuthorization()
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services
            .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .PostConfigure<IOptions<DiscordAuthenticationOptions>>(ConfigureJwtBearerOptions);

        services.AddOptions<DiscordAuthenticationOptions>()
            .PostConfigure<ILogger<DiscordAuthenticationOptions>>(ConfigureDevelopmentCertificate);

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
        IOptions<DiscordAuthenticationOptions> discordOptions)
    {
        var securityKey = new X509SecurityKey(discordOptions.Value.SigningCertificate);

        options.TokenValidationParameters.IssuerSigningKey = securityKey;
        options.TokenValidationParameters.ValidateIssuer = false;
        options.TokenValidationParameters.ValidateAudience = false;
        options.TokenValidationParameters.NameClaimType = JwtRegisteredClaimNames.Name;
        options.TokenValidationParameters.RoleClaimType = ClaimTypes.Role;
    }

    private static void ConfigureDiscordAuthentication(
        DiscordAuthenticationOptions options,
        IConfiguration configuration)
    {
        configuration.GetSection("DiscordAuthentication").Bind(options);

        // Configuration values from key vault
        options.Base64SigningCertificate = configuration["SigningCertificate"];
    }

    private static void ConfigureDevelopmentCertificate(DiscordAuthenticationOptions options, ILogger<DiscordAuthenticationOptions> logger)
    {
        if (options.Base64SigningCertificate is null)
        {
            logger.LogWarning("No signing certificate was provided, generating certificate for development purposes");

            var rsa = RSA.Create();
            var req = new CertificateRequest("cn=test", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            var certificate = req.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(5));
            options.Base64SigningCertificate = Convert.ToBase64String(certificate.Export(X509ContentType.Pfx, ""));
        }
    }

    private static void ConfigureDiscordClient(DiscordClientOptions options, IConfiguration configuration)
    {
        configuration.GetSection("Discord").Bind(options);

        // Configuration values from key vault
        options.ClientId = configuration["DiscordClientId"] ?? throw new NullReferenceException();
        options.ClientSecret = configuration["DiscordClientSecret"] ?? throw new NullReferenceException();
        options.BotToken = configuration["DiscordBotToken"] ?? throw new NullReferenceException();
    }
}