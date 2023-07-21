using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Seatpicker.Domain;
using Seatpicker.IntegrationTests.TestAdapters;
using Shared;
using Xunit;

namespace Seatpicker.IntegrationTests;

public abstract class IntegrationTestBase : IDisposable
{
    private readonly TestWebApplicationFactory factory;

    protected IntegrationTestBase(TestWebApplicationFactory factory)
    {
        this.factory = factory;
    }

    protected TService GetService<TService>()
        where TService : notnull
    {
        return factory.Services.GetRequiredService<TService>();
    }

    protected HttpClient GetClient(TestIdentity identity)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", identity.Token);

        return client;
    }

    protected Task<TestIdentity> CreateIdentity(params Role[] roles)
    {
        var identityGenerator = factory.Services.GetRequiredService<IdentityGenerator>();
        return identityGenerator.GenerateWithRoles(roles);
    }

    protected HttpClient GetAnonymousClient() => factory.CreateClient();

    protected InterceptingHttpMessageHandler InterceptingHttpMessageHandler =>
        factory.Services.GetRequiredService<InterceptingHttpMessageHandler>();

    protected void SetupAggregates(params AggregateBase[] aggregates)
    {
        var repository = factory.Services.GetRequiredService<TestAggregateRepository>();
        foreach (var aggregate in aggregates)
        {
            repository.Aggregates[aggregate.Id] = aggregate;
        }
    }

    protected IEnumerable<TAggregate> GetCommittedAggregates<TAggregate>()
        where TAggregate : AggregateBase
    {
        var repository = factory.Services.GetRequiredService<TestAggregateRepository>();
        return repository.Aggregates.Values.OfType<TAggregate>();
    }

    public void Dispose()
    {
        var repository = factory.Services.GetRequiredService<TestAggregateRepository>();
        repository.Aggregates.Clear();
    }
}