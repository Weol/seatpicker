namespace Seatpicker.SeatContext.Registration.Ports;

public interface ICreateJwtToken {
    public Task<string> ForUser(User user);
}