using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Seatpicker.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSingletonPortMapping<TPort, TImplementation>(this IServiceCollection services)
        where TImplementation : class, TPort where TPort : class
    {
        services.TryAddSingleton<TImplementation>();

        return services.AddSingleton<TPort>(provider => provider.GetRequiredService<TImplementation>());
    }

    public static IServiceCollection AddScopedPortMapping<TPort, TImplementation>(this IServiceCollection services)
        where TImplementation : class, TPort where TPort : class
    {
        services.TryAddScoped<TImplementation>();

        return services.AddScoped<TPort>(provider => provider.GetRequiredService<TImplementation>());
    }
}