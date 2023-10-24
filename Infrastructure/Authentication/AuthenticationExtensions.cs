using System.Security.Claims;
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
        options.Base64SigningCertificate = configuration["SigningCertificate"] ?? throw new NullReferenceException();
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