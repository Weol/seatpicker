using System.Security.Cryptography.X509Certificates;
using Application.Authentication;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Seatpicker.Adapters;

internal class AuthenticationCertificateProvider : IAuthenticationCertificateProvider
{
    private readonly SecretClient secretClient;
    private readonly string secretName;
    
    public AuthenticationCertificateProvider(SecretClient secretClient, IOptionsSnapshot<Options> options)
    {
        this.secretClient = secretClient;
    }

    public async Task<X509Certificate2?> Get()
    {
        var response = await secretClient.GetSecretAsync(secretName);
        var value = response.Value.Value;

        if (value is null) return null;
        
        var bytes = Convert.FromBase64String(value);
        return new X509Certificate2(bytes);
    }

    public class Options 
    {
        public string SecretName { get; set; } = null!;
    }
}

internal static class AuthenticationCertificateProviderExtensions
{
    public static IServiceCollection AddAuthenticationCertificateProvider(this IServiceCollection services, Action<AuthenticationCertificateProvider.Options> configureAction)
    {
        services.Configure(configureAction);
        
        return services.AddScoped<IAuthenticationCertificateProvider, AuthenticationCertificateProvider>();
    }
}