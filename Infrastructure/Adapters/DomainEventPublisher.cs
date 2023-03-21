using MassTransit;
using Shared;

namespace Seatpicker.Infrastructure.Adapters;

internal class DomainEventPublisher : IDomainEventPublisher
{
    private readonly IBus bus;

    public DomainEventPublisher(IBus bus)
    {
        this.bus = bus;
    }

    public Task Publish(IDomainEvent domainEvent)
    {
        return bus.Publish(domainEvent);
    }
}

internal static class DomainEventPublisherExtensions
{
    internal static IServiceCollection AddDomainEventPublisher( this IServiceCollection services)
    {
        return services.AddSingleton<IDomainEventPublisher, DomainEventPublisher>();
    }
}
