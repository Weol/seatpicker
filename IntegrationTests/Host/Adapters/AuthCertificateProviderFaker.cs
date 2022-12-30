using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Azure;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Seatpicker.IntegrationTests.Host.Adapters;

public class AuthCertificateProviderFaker
{
    private readonly Host host;
    
    public AuthCertificateProviderFaker(Host host)
    {
        this.host = host;
    }
    
    public X509Certificate2 SetupAuthCertificate()
    {
        var factory = host.Services.GetRequiredService<IAzureClientFactory<SecretClient>>();

        var req = new CertificateRequest("CN=Test", RSA.Create(), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        var authCertificate = req.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(1));
        var b64AuthCertificate = Convert.ToBase64String(
            authCertificate.Export(X509ContentType.Pfx, ""));
        
        var secretClient = Substitute.For<SecretClient>();
        secretClient.GetSecretAsync(Arg.Any<string>())
            .Returns(info =>
            {
                var secretName = (string)info[0];
                return Response.FromValue(new KeyVaultSecret(secretName, b64AuthCertificate), null!);
            });
        
        var clientName = host.AdapterConfiguration.KeyvaultUri.ToString();
        factory.CreateClient(clientName).Returns(secretClient);

        return authCertificate;
    }
}

public static class AuthCertificateProviderFakerExtensions
{
    public static IServiceCollection AddAuthCertificateProviderFaker(this IServiceCollection services)
    {
        return services.AddSingleton(_ => Substitute.For<IAzureClientFactory<SecretClient>>());
    }
}