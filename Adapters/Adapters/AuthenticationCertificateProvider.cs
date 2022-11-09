using System.Security.Cryptography.X509Certificates;
using Application.Authentication.Ports;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Seatpicker.Adapters.Adapters;

internal class AuthenticationCertificateProvider : IAuthenticationCertificateProvider
{
    private readonly SecretClient secretClient;
    private readonly string secretName;
    
    public AuthenticationCertificateProvider(SecretClient secretClient, IOptionsSnapshot<Options> options)
    {
        this.secretClient = secretClient;
        secretName = options.Value.SecretName;
    }

    public async Task<X509Certificate2> Get()
    {
        var response = await secretClient.GetSecretAsync(secretName);
        var value = response.Value.Value;

        if (value is null) throw new NullReferenceException(nameof(value));
        
        var bytes = Convert.FromBase64String(value);
        return new X509Certificate2(bytes);
    }

    public class Options 
    {
        public string SecretName { get; set; } = null!;
        public Uri KeyvaultUri { get; set; } = null!;
    }
}

internal static class AuthenticationCertificateProviderExtensions
{
    internal static IServiceCollection AddAuthenticationCertificateProvider(this IServiceCollection services, Action<AuthenticationCertificateProvider.Options> configureAction)
    {
        services.Configure(configureAction);
        
        services.AddAzureClients(builder =>
        {
            builder.AddClient<SecretClient, AuthenticationCertificateProvider.Options>(options 
                => new SecretClient(options.KeyvaultUri, new DefaultAzureCredential()));
        });
        
        return services.AddScoped<IAuthenticationCertificateProvider, AuthenticationCertificateProvider>();
    }
}