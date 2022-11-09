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
    
    public static IServiceCollection AddAdapters(this IServiceCollection services, IConfiguration configuration)
    {
        Configuration = configuration;
        
        return services
            .AddUserStore(GetUserStoreOptions())
            .AddAuthenticationCertificateProvider(ConfigureAutheneticationCertificateProvider);
    }

    private static void ConfigureAutheneticationCertificateProvider(AuthenticationCertificateProvider.Options options)
    {
        throw new NotImplementedException();
    }

    private static TableStorageOptions GetUserStoreOptions()
    {
        return new TableStorageOptions
        {
            Uri = new Uri(Configuration["TableStorageUri"]),
            TableName = Configuration["UserTableName"],
        };
    }
}
