using Seatpicker.UserContext.Domain;

namespace Seatpicker.UserContext.Application.UserToken.Ports;

public interface ILookupUser
{
    Task<User?> Lookup(string id);
}
