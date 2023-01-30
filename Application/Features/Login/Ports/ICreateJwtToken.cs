using System.Security.Cryptography.X509Certificates;
using Seatpicker.Domain;

namespace Seatpicker.Application.Features.Login.Ports;

public interface ICreateJwtToken
{
    public Task<string> CreateFor(User user, X509Certificate2 certificate);
}