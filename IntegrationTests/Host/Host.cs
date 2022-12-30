using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using Seatpicker.Adapters;
using Seatpicker.Application;
using Seatpicker.IntegrationTests.Host.Adapters;
using Seatpicker.UserContext;

namespace Seatpicker.IntegrationTests.Host;

public partial class Host
{
    public Host()
    {
        IdentityModelEventSource.ShowPII = true;
        
        var services = new ServiceCollection();

        services.AddSingleton(this);
        
        services
            .AddAdapters(AdapterConfiguration)
            .AddApplication()
            .AddUserContext(UserContextConfiguration);

        services.AddHttpRequestFaker()
            .AddAuthCertificateProviderFaker()
            .AddDiscordClientFaker()
            .AddUserStoreFaker();

        Services = services.BuildServiceProvider();
    }

    public ServiceProvider Services { get; }

    public IAdapterConfiguration AdapterConfiguration { get; } = new TestAdapterConfiguration();

    public IUserContextConfiguration UserContextConfiguration { get; } = new TestUserContextConfiguration();
    
    private class TestAdapterConfiguration : IAdapterConfiguration
    {
        public string ClientId => "TestClientId";
        public string ClientSecret => "TestClientSecret";
        public Uri KeyvaultUri => new("http://test.keyvault.local");
        public Guid LanId => Guid.NewGuid();
        public string StorageEndpoint => "http://test.storage.local";
        public Uri DiscordBaseUri => new("http://discord.local/api/v10");
        public Uri DiscordRedirectUri => new("http://test.discordredirect.local");
    }

    private class TestUserContextConfiguration : IUserContextConfiguration
    {
        public string ClientId => "TestClientId";
    }
}

