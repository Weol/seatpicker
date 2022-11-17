using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Seatpicker.Adapters.Adapters;
using Seatpicker.Adapters.Common;
using Seatpicker.Domain;

namespace Seatpicker.Adapters;

public static class AdapterExtensions
{
    public static IConfiguration Configuration { get; set; }
    
    public static IServiceCollection AddAdapters(this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
    {
        Configuration = configuration;

        return services
            .AddUserStore(GetUserStoreOptions())
            .AddAuthenticationCertificateProvider(ConfigureAuthenticationCertificateProvider, isDevelopment)
            .AddLanIdentityProvider(ConfigureLanIdentityProvider);
    }

    private static void ConfigureLanIdentityProvider(LanIdentityProvider.Options options)
    {
        options.LanId = Guid.Parse(Configuration["LanId"]);
    }
    
    private static void ConfigureAuthenticationCertificateProvider(AuthCertificateProvider.Options options)
    {
        options.SecretName = "AuthenticationCertificate";
        options.KeyvaultUri = new Uri(Configuration["KeyvaultUri"]);
    }

    private static TableStorageOptions GetUserStoreOptions()
    {
        return new TableStorageOptions
        {
            Endpoint = Configuration["StorageEndpoint"],
            TableName = "Users",
        };
    }
}
