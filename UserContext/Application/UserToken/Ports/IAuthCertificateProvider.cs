using System.Security.Cryptography.X509Certificates;

namespace Seatpicker.Domain.Application.UserToken.Ports;

public interface IAuthCertificateProvider
{
    Task<X509Certificate2> Get();
}