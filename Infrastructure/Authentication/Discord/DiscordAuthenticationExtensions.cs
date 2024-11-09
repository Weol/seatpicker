namespace Seatpicker.Infrastructure.Authentication.Discord;

public static class DiscordAuthenticationExtensions
{
    public static IServiceCollection AddDiscordAuthentication(
        this IServiceCollection services,
        Action<AuthenticationOptions, IConfiguration> configureAuthAction)
    {
        services.AddOptions<AuthenticationOptions>()
            .Configure(configureAuthAction)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services
            .AddScoped<DiscordAuthenticationService>();
    }
}