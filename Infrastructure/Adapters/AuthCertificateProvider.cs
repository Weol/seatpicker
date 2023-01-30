using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Seatpicker.Application.Features.Login.Ports;

namespace Seatpicker.Infrastructure.Adapters;

internal class AuthCertificateProvider : IAuthCertificateProvider
{
    private readonly SecretClient secretClient;
    private readonly string secretName;

    public AuthCertificateProvider(IAzureClientFactory<SecretClient> secretClientFactory, IOptions<Options> options)
    {
        secretClient = secretClientFactory.CreateClient(options.Value.Uri.ToString());
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
        public Uri Uri { get; set; } = null!;
        public ClientSecretCredentials? Credentials { get; set; } = null!;

        public class ClientSecretCredentials
        {
            public Guid TenantId { get; set; }
            public Guid ClientId { get; set; }
            public string ClientSecret { get; set; } = null!;
        }
    }
}

internal static class AuthenticationCertificateProviderExtensions
{
    internal static IServiceCollection AddAuthenticationCertificateProvider(this IServiceCollection services,
        Action<AuthCertificateProvider.Options> configureAction)
    {
        services.Configure(configureAction);

        services.AddAzureClients(builder =>
        {
            var options = new AuthCertificateProvider.Options();
            configureAction(options);

            if (options.Credentials is not null)
            {
                builder.AddSecretClient(options.Uri)
                    .WithCredential(new ClientSecretCredential(options.Credentials.TenantId.ToString(),
                        options.Credentials.ClientId.ToString(),
                        options.Credentials.ClientSecret.ToString()))
                    .WithName(options.Uri.ToString());
            }
            else
            {
                builder.AddSecretClient(options.Uri)
                    .WithName(options.Uri.ToString());
            }
        });

        return services.AddScoped<IAuthCertificateProvider, AuthCertificateProvider>();
    }
}