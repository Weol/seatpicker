namespace Application.Ports;

public interface ILanIdentityProvider
{
    Task<Guid> GetCurrentLanId();
}