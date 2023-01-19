namespace Seatpicker.SeatContext.Layout.Ports;

public interface IGetLayoutBackground
{
    Task<byte[]> Get();
}
