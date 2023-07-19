using System.Text;
using Marten;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Seatpicker.Application.Features;
using Seatpicker.Application.Features.Login.Ports;
using Seatpicker.IntegrationTests.TestAdapters;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests;

public class TestWebApplicationFactory : WebApplicationFactory<Infrastructure.Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder
            .UseEnvironment("Test")
            .ConfigureServices(services =>
            {
                services.RemoveAll<IAuthCertificateProvider>();
                services.AddSingleton<IAuthCertificateProvider, TestAuthCertificateProvider>();

                services.RemoveAll<IAggregateRepository>();
                services.AddSingleton<TestAggregateRepository>();
                services.AddSingleton<IAggregateRepository>(provider => provider.GetRequiredService<TestAggregateRepository>());

                services.AddSingleton<IdentityGenerator>();
            });
    }
}