using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Seatpicker.Application.Features.Token.Ports;

namespace Seatpicker.IntegrationTests.TestAdapters;

public class TestAuthCertificateProvider : IAuthCertificateProvider
{
    private readonly X509Certificate2 certificate = GenerateSelfSignedCertificate();

    private static X509Certificate2 GenerateSelfSignedCertificate()
    {
        var rsa = RSA.Create();
        var req = new CertificateRequest("cn=test", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        return req.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(5));
    }

    public Task<X509Certificate2> Get() => Task.FromResult(certificate);
}