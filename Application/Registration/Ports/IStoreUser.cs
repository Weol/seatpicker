namespace Seatpicker.SeatContext.Registration.Ports;

public interface IStoreUser
{
    Task Store(User user);
}