using System.Security.Cryptography.X509Certificates;

namespace Seatpicker.UserContext.Application.UserToken.Ports;

public interface IAuthCertificateProvider
{
    Task<X509Certificate2> Get();
}