using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Seatpicker.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPortMapping<TPort, TImpl>(this IServiceCollection serviceCollection)
        where TPort : class
        where TImpl : class, TPort
    {
        serviceCollection.TryAddSingleton<TImpl>();

        return serviceCollection
            .AddSingleton<TPort>(provider => provider.GetRequiredService<TImpl>());
    }
}