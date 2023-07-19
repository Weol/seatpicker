using System.Security.Cryptography.X509Certificates;
using Shared;

namespace Seatpicker.Application.Features.Login.Ports;

public interface IAuthCertificateProvider : IPort
{
    Task<X509Certificate2> Get();
}