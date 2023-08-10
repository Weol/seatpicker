using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Seatpicker.Infrastructure.Authentication.Discord;

namespace Seatpicker.Infrastructure.Authentication;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddSeatpickerAuthentication(
        this IServiceCollection services)
    {
        services
            .AddDiscordLogin(ConfigureDiscordAuthentication)
            .AddAuthorization()
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer();

        services
            .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .PostConfigure<IOptions<DiscordAuthenticationOptions>>(ConfigureJwtBearerOptions);

        return services;
    }

    public static IApplicationBuilder UseSeatpickerAuthentication(this IApplicationBuilder app)
    {
        return app
            .UseAuthentication()
            .UseAuthorization()
            .MapDiscordAuthenticationEndpoints();
    }

    private static void ConfigureJwtBearerOptions(
        JwtBearerOptions options,
        IOptions<DiscordAuthenticationOptions> discordOptions)
    {
        var securityKey = new X509SecurityKey(discordOptions.Value.SigningCertificate);

        options.TokenValidationParameters.ValidIssuer = discordOptions.Value.Issuer;
        options.TokenValidationParameters.IssuerSigningKey = securityKey;
        options.TokenValidationParameters.ValidateAudience = false;
    }

    private static void ConfigureDiscordAuthentication(
        DiscordAuthenticationOptions options,
        IConfiguration configuration)
    {
        configuration.GetSection("DiscordAuthentication").Bind(options);

        // Configuration values from key vault
        options.Base64SigningCertificate = configuration["SigningCertificate"] ?? throw new NullReferenceException();
    }
}