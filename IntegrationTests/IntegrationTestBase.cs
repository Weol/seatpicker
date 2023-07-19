using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Seatpicker.Domain;
using Seatpicker.IntegrationTests.TestAdapters;
using Shared;
using Xunit;

namespace Seatpicker.IntegrationTests;

public abstract class IntegrationTestBase
{
    private readonly TestWebApplicationFactory factory;

    private readonly HttpClient anonymousClient;

    protected IntegrationTestBase(TestWebApplicationFactory factory)
    {
        this.factory = factory;

        anonymousClient = this.factory.CreateClient();
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

    protected HttpClient GetAnonymousClient() => anonymousClient;

    protected void SetupAggregates(params AggregateBase[] aggregates)
    {
        var repository = factory.Services.GetRequiredService<TestAggregateRepository>();
        foreach (var aggregate in aggregates)
        {
            repository.Aggregates[aggregate.Id] = aggregate;
        }
    }

    protected void CleanupAggregates(params AggregateBase[] aggregates)
    {
        var repository = factory.Services.GetRequiredService<TestAggregateRepository>();
        repository.Aggregates.Clear();
    }
}