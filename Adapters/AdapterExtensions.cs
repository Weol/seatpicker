using Microsoft.Extensions.DependencyInjection;
using Seatpicker.Adapters.Adapters;

namespace Seatpicker.Adapters;

public interface IAdapterConfiguration
{
    Uri DiscordBaseUri { get; }
    Uri DiscordRedirectUri { get; }
    string ClientId { get; }
    string ClientSecret { get; }
    Guid LanId { get; }
    Uri KeyvaultUri { get; }
    string StorageEndpoint { get; }
}

public static class AdapterExtensions
{
    private static IAdapterConfiguration Configuration { get; set; } = null!;

    public static IServiceCollection AddAdapters(this IServiceCollection services, IAdapterConfiguration configuration)
    {
        Configuration = configuration;

        return services
            .AddUserStore(ConfigureUserStore)
            .AddAuthenticationCertificateProvider(ConfigureAuthenticationCertificateProvider)
            .AddLanIdentityProvider(ConfigureLanIdentityProvider)
            .AddDiscordClient(ConfigureDiscordClient);
    }

    private static void ConfigureDiscordClient(DiscordClientOptions options)
    {
        options.BaseUri = Configuration.DiscordBaseUri;
        options.RedirectUri = Configuration.DiscordRedirectUri;
        options.ClientId = Configuration.ClientId;
        options.ClientSecret = Configuration.ClientSecret;
    }

    private static void ConfigureLanIdentityProvider(LanIdentityProvider.Options options)
    {
        options.LanId = Configuration.LanId;
    }

    private static void ConfigureAuthenticationCertificateProvider(AuthCertificateProvider.Options options)
    {
        options.SecretName = "AuthenticationCertificate";
        options.KeyvaultUri = Configuration.KeyvaultUri;
    }

    private static void ConfigureUserStore(UserStore.Options options)
    {
        options.Endpoint = Configuration.StorageEndpoint;
        options.TableName = "Users";
    }
}