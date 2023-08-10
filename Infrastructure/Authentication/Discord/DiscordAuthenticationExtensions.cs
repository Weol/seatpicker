using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Authentication;

namespace Seatpicker.Infrastructure.Authentication.Discord;

public static class DiscordAuthenticationExtensions
{
    public static IServiceCollection AddDiscordLogin(
        this IServiceCollection services,
        Action<DiscordAuthenticationOptions, IConfiguration> configureAction)
    {
        return services
            .AddValidatedOptions(configureAction)
            .AddScoped<DiscordJwtTokenCreator>();
    }

    public static IApplicationBuilder MapDiscordAuthenticationEndpoints(this IApplicationBuilder app)
    {
        app.UseEndpoints(
            asd =>
            {
                asd.MapPost("discord/login", DiscordLoginEndpoints.Login);
                asd.MapPost("discord/renew", DiscordLoginEndpoints.Renew);
            });

        return app;
    }
}