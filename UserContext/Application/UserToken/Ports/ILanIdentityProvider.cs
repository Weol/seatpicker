namespace Seatpicker.Domain.Application.UserToken.Ports;

public interface ILanIdentityProvider
{
    Task<Guid> GetCurrentLanId();
}
