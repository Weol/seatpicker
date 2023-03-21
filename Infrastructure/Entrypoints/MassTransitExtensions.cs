using System.Reflection;
using System.Text.RegularExpressions;
using Azure.Messaging.ServiceBus;
using MassTransit;
using MassTransit.Transports;

namespace Seatpicker.Infrastructure.Entrypoints;

public class MassTransitOptions
{
    public string ServiceBusEndpoint { get; set; } = null!;
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
                        if (Uri.TryCreate(options.ServiceBusEndpoint, UriKind.Absolute, out var uri))
                            cfg.Host(uri);
                        else
                            cfg.Host(options.ServiceBusEndpoint);

                        cfg.ConfigureEndpoints(context);
                    });

            });
        return services;
    }

    public static void AddAllConsumers(this IBusRegistrationConfigurator configurator)
    {
        var consumers = typeof(Program)
            .Assembly
            .GetTypes()
            .Where(type => !type.IsAbstract && type.IsAssignableTo(typeof(IConsumer)))
            .ToArray();

        configurator.AddConsumers(consumers);
    }
}