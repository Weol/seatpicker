namespace Seatpicker.Domain.UserRegistration.Ports;

public interface IStoreUser
{
    Task Store(User user);
}