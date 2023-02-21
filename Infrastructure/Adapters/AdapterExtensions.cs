using Seatpicker.Infrastructure.Adapters.Database;
using Seatpicker.Infrastructure.Adapters.DiscordClient;

namespace Seatpicker.Infrastructure.Adapters;

public static class AdapterExtensions
{
    private static IConfiguration Config { get; set; } = null!;

    public static IServiceCollection AddAdapters(this IServiceCollection services, IConfiguration configuration)
    {
        Config = configuration;

        return services
            .AddDatabase()
            .AddAuthenticationCertificateProvider(ConfigureAuthenticationCertificateProvider)
            .AddDiscordClient(ConfigureDiscordClient);
    }

    private static void ConfigureDiscordClient(DiscordClientOptions options)
    {
        Config.GetSection("Discord").Bind(options);
    }

    private static void ConfigureAuthenticationCertificateProvider(AuthCertificateProvider.Options options)
    {
        Config.GetSection("Keyvault").Bind(options);
    }
}