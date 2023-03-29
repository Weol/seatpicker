using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Seatpicker.Infrastructure;

namespace Seatpicker.IntegrationTests.Host;

public class WebApplicationFactory : Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.UseEnvironment("Testing");
    }

    private static IServiceCollection ReplaceWithSingleton<TInterface, TImplementation>(IServiceCollection services)
        where TImplementation : class, TInterface
        where TInterface : class
    {
        var descriptor = services.Single(d => d.ServiceType == typeof(TInterface));

        services.Remove(descriptor);
        services.AddSingleton<TInterface, TImplementation>();

        return services;
    }
}