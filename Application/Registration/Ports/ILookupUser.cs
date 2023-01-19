namespace Seatpicker.SeatContext.Registration.Ports;

public interface ILookupUser
{
    Task<User?> Lookup(string id);
}
