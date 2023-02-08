using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using NSubstitute;
using Seatpicker.Application;
using Seatpicker.Application.Features.Login.Ports;

namespace Seatpicker.IntegrationTests;

public class Host
{
    public ServiceProvider Services { get; }

    public Host()
    {
        IdentityModelEventSource.ShowPII = true;

        var services = new ServiceCollection();

        services.AddSingleton(this);

        services
            .AddLogging()
            .AddApplication();

        AddMock<IDiscordLookupUser>(services);
        AddMock<IDiscordAccessTokenProvider>(services);
        AddMock<IAuthCertificateProvider>(services);

        Services = services.BuildServiceProvider();

        SetupAuthCertificateProvider();
    }

    private void SetupAuthCertificateProvider()
    {
        var certificateProvider = Services.GetRequiredService<IAuthCertificateProvider>();

        var rsa = RSA.Create(); // generate asymmetric key pair
        var req = new CertificateRequest("cn=test", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        var cert = req.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(5));

        certificateProvider.Get().Returns(cert);
    }

    private static void AddMock<TType>(IServiceCollection serviceCollection)
        where TType : class
    {
        serviceCollection.AddSingleton<TType>(Substitute.For<TType>());
    }
}

