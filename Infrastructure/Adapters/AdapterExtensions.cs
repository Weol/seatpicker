using Seatpicker.Infrastructure.Adapters.Database;
using Seatpicker.Infrastructure.Adapters.DiscordClient;

namespace Seatpicker.Infrastructure.Adapters;

public static class AdapterExtensions
{
    public static IServiceCollection AddAdapters(this IServiceCollection services)
    {
        return services
            .AddDatabase(ConfigureDatabase)
            .AddDiscordClient(ConfigureDiscordClient);
    }

    private static void ConfigureDatabase(DatabaseOptions options, IConfiguration configuration)
    {
        configuration.GetSection("Database").Bind(options);
    }

    private static void ConfigureDiscordClient(DiscordClientOptions options, IConfiguration configuration)
    {
        configuration.GetSection("Discord").Bind(options);

        // Configuration values from key vault
        options.ClientId = configuration["DiscordClientId"] ?? throw new NullReferenceException();
        options.ClientSecret = configuration["DiscordClientSecret"] ?? throw new NullReferenceException();
    }
}