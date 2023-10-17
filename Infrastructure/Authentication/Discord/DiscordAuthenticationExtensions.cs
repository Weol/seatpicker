using Microsoft.OpenApi.Writers;
using Seatpicker.Infrastructure.Authentication.Discord.DiscordClient;

namespace Seatpicker.Infrastructure.Authentication.Discord;

public static class DiscordAuthenticationExtensions
{
    public static IServiceCollection AddDiscordLogin(
        this IServiceCollection services,
        Action<DiscordAuthenticationOptions, IConfiguration> configureAuthAction,
        Action<DiscordClientOptions, IConfiguration> configureClientAction)
    {
        services.AddOptions<DiscordAuthenticationOptions>()
            .Configure(configureAuthAction)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services
            .AddDiscordClient(configureClientAction)
            .AddScoped<DiscordAuthenticationService>()
            .AddScoped<DiscordJwtTokenCreator>();
    }
}