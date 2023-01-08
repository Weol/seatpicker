namespace Seatpicker.SeatContext.Domain.Layout.Ports;

public interface IGetLayoutBackground
{
    Task<byte[]> Get();
}
