using System.Security.Cryptography.X509Certificates;
using Azure.Identity;
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
                .WithCredential(new ClientSecretCredential("efe9fc32-844b-45fe-a128-553444bb9a67", "f7f9897b-7ad1-4f52-a087-78e079f87a35", "5b~8Q~3rrlUKzZ2Il~f~qtBrAdMYYq.ro543Kcy~"))
                .WithName(options.KeyvaultUri.ToString());
        });
        
        return services.AddScoped<IAuthCertificateProvider, AuthCertificateProvider>();
    }
}