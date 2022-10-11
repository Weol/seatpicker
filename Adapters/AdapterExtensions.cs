using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Seatpicker.Adapters;

public static class AdapterExtensions
{
    public static IServiceCollection AddAdapters(this IServiceCollection services, AdaptersConfiguration config)
    {
        return services
            .AddAuthenticationCertificateProvider(options => options.SecretName = config.AuthenticationCertificateSecretName);
    }
}
