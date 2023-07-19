using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests;

public abstract class IntegrationTestBase
{
    private readonly ITestOutputHelper testOutputHelper;
    private readonly TestWebApplicationFactory factory;

    private readonly HttpClient anonymousClient;

    protected IntegrationTestBase(TestWebApplicationFactory factory, ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
        this.factory = factory;

        anonymousClient = this.factory.CreateClient();
    }

    protected HttpClient GetClient(TestIdentity identity)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", identity.Token);

        return client;
    }

    protected HttpClient GetAnonymousClient() => anonymousClient;
}
