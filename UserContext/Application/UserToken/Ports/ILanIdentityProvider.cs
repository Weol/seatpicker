namespace Seatpicker.UserContext.Application.UserToken.Ports;

public interface ILanIdentityProvider
{
    Task<Guid> GetCurrentLanId();
}
