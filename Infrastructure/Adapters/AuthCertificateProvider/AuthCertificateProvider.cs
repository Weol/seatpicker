using System.Security.Cryptography.X509Certificates;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Options;
using Seatpicker.Application.Features.Login.Ports;

namespace Seatpicker.Infrastructure.Adapters.AuthCertificateProvider;

internal class AuthCertificateProvider : IAuthCertificateProvider
{
    private readonly SecretClient secretClient;
    private readonly string secretName;

    public AuthCertificateProvider(IAzureClientFactory<SecretClient> secretClientFactory, IOptions<AuthCertificateProviderOptions> options)
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
}