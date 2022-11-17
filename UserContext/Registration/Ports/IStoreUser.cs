namespace Seatpicker.Domain.Registration.Ports;

public interface IStoreUser
{
    Task Store(User user);
}