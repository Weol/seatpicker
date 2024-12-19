using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Seatpicker.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPortMapping<TPort, TImpl>(this IServiceCollection services)
        where TPort : class
        where TImpl : class, TPort
    {
        services.TryAddSingleton<TImpl>();

        return services
            .AddSingleton<TPort>(provider => provider.GetRequiredService<TImpl>());
    }

    public static IServiceCollection AddValidatedOptions<TOptions>(
        this IServiceCollection services,
        Action<TOptions, IConfiguration> configureAction)
        where TOptions : class
    {
        services.AddOptions<TOptions>()
            .Configure(configureAction)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}