using Seatpicker.Domain;

namespace Application.Ports;

public interface ILookupUser
{
    Task<User> Lookup(string id);
}