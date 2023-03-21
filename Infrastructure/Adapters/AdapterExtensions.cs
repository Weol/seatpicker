using Seatpicker.Infrastructure.Adapters.DiscordClient;

namespace Seatpicker.Infrastructure.Adapters;

public static class AdapterExtensions
{
    private static IConfiguration Config { get; set; } = null!;

    public static IServiceCollection AddAdapters(this IServiceCollection services, IConfiguration configuration)
    {
        Config = configuration;

        return services
            .AddSeatRepository(ConfigureSeatRepository)
            .AddAuthCertificateProvider(ConfigureAuthCertificateProvider)
            .AddJwtTokenCreator()
            .AddDiscordClient(ConfigureDiscordClient);
    }

    private static void ConfigureSeatRepository(SeatRepository.Options options)
    {
        Config.GetSection("SeatRepository").Bind(options);
    }

    private static void ConfigureDiscordClient(DiscordClientOptions options)
    {
        Config.GetSection("Discord").Bind(options);
    }

    private static void ConfigureAuthCertificateProvider(AuthCertificateProvider.Options options)
    {
        Config.GetSection("AuthCertificateProvider").Bind(options);
    }
}