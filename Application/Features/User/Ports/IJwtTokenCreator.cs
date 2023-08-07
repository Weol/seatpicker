using System.Security.Cryptography.X509Certificates;
using Seatpicker.Domain;

namespace Seatpicker.Application.Features.Token.Ports;

public interface IJwtTokenCreator
{
    Task<string> CreateFor(User user, X509Certificate2 authCertificate, ICollection<Role> claims);
}