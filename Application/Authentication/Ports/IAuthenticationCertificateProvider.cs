using System.Security.Cryptography.X509Certificates;

namespace Application.Authentication.Ports;

public interface IAuthenticationCertificateProvider
{
    Task<X509Certificate2> Get();
}