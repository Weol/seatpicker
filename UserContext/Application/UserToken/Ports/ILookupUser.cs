using Seatpicker.Domain.Domain;

namespace Seatpicker.Domain.Application.UserToken.Ports;

public interface ILookupUser
{
    Task<User?> Lookup(string id);
}
