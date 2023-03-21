namespace Shared;

public interface IDomainEventPublisher
{
    Task Publish(IDomainEvent domainEvent);
}