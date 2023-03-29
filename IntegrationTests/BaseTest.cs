using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Seatpicker.Application.Features.Login;
using Seatpicker.Application.Features.Login.Ports;
using Seatpicker.IntegrationTests.Host;
using Xunit;

namespace Seatpicker.IntegrationTests;

public class BaseTest : IClassFixture<WebApplicationFactory>
{
    private readonly WebApplicationFactory factory;

    public BaseTest(WebApplicationFactory factory)
    {
        this.factory = factory;
        HttpClient = this.factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = true,
        });
    }

    public HttpClient HttpClient { get; }

    public T GetService<T>()
        where T : notnull
    {
        return factory.Services.GetRequiredService<T>();
    }
}