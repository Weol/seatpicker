using System.Security.Cryptography.X509Certificates;

namespace Seatpicker.Application.Features.Login.Ports;

public interface IAuthCertificateProvider
{
    Task<X509Certificate2> Get();
}