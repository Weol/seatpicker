using System.Reflection;
using System.Text.RegularExpressions;
using Azure.Messaging.ServiceBus;
using MassTransit;
using MassTransit.Transports;

namespace Seatpicker.Infrastructure.Entrypoints;

public class MassTransitOptions
{
    public string ServiceBusConnectionString { get; set; } = null!;
}

public static class MassTransitExtensions
{
    public static IServiceCollection AddBus(this IServiceCollection services, MassTransitOptions options)
    {
        services.AddMassTransit(
            configurator =>
            {
                configurator.AddAllConsumers();

                configurator.UsingAzureServiceBus(
                    (context, cfg) =>
                    {
                        cfg.Host(options.ServiceBusConnectionString);

                        cfg.ConfigureEndpoints(context);
                    });

            });
        return services;
    }

    private static void AddAllConsumers(this IBusRegistrationConfigurator configurator)
    {
        var consumers = typeof(Program)
            .Assembly
            .GetTypes()
            .Where(type => !type.IsAbstract && type.IsAssignableTo(typeof(IConsumer)))
            .ToArray();

        configurator.AddConsumers(consumers);
    }
}