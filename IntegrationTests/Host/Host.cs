using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using Seatpicker.IntegrationTests.Host.Adapters;

namespace Seatpicker.IntegrationTests.Host;

public partial class Host
{
    public Host()
    {
        IdentityModelEventSource.ShowPII = true;
        
        var services = new ServiceCollection();

        services.AddSingleton(this);
        
        services.AddHttpRequestFaker()
            .AddAuthCertificateProviderFaker()
            .AddDiscordClientFaker()
            .AddUserStoreFaker();

        Services = services.BuildServiceProvider();
    }

    public ServiceProvider Services { get; }
}

