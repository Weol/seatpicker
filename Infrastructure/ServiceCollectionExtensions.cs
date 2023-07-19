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

    public static IServiceCollection AddValidatedOptions<TOptions>(
        this IServiceCollection serviceCollection,
        Action<TOptions> configureAction)
        where TOptions : class
    {
        serviceCollection.AddOptions<TOptions>()
            .Configure(configureAction)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return serviceCollection;
    }
}