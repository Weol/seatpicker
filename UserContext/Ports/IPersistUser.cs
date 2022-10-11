namespace Seatpicker.Domain.Ports;

public interface IPersistUser
{
    Task Persist(User user);
}