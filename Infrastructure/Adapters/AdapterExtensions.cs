﻿namespace Seatpicker.Infrastructure.Adapters;

public static class AdapterExtensions
{
    private static IConfiguration Config { get; set; } = null!;

    public static IServiceCollection AddAdapters(this IServiceCollection services, IConfiguration configuration)
    {
        Config = configuration;

        return services
            .AddDomainEventPublisher()
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
        Config.GetSection("DiscordClient").Bind(options);
    }

    private static void ConfigureAuthCertificateProvider(AuthCertificateProvider.Options options)
    {
        Config.GetSection("AuthCertificateProvider").Bind(options);
    }
}