using System.Security.Cryptography.X509Certificates;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Seatpicker.Domain.Application.UserToken.Ports;

namespace Seatpicker.Adapters.Adapters;

internal class AuthCertificateProvider : IAuthCertificateProvider
{
    private readonly SecretClient secretClient;
    private readonly string secretName;
    
    public AuthCertificateProvider(IAzureClientFactory<SecretClient> secretClientFactory, IOptions<Options> options)
    {
        secretClient = secretClientFactory.CreateClient(options.Value.KeyvaultUri.ToString());
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
    internal static IServiceCollection AddAuthenticationCertificateProvider(this IServiceCollection services, Action<AuthCertificateProvider.Options> configureAction)
    {
        services.Configure(configureAction);
        
        services.AddAzureClients(builder =>
        {
            var options = new AuthCertificateProvider.Options();
            configureAction(options);
            
            builder.AddSecretClient(options.KeyvaultUri)
                .WithName(options.KeyvaultUri.ToString());
        });
        
        return services.AddScoped<IAuthCertificateProvider, AuthCertificateProvider>();
    }
}