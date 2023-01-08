namespace Seatpicker.SeatContext.Domain.Layout.Ports;

public interface IGetTables
{
    Task<IEnumerable<Table>> Get();
}
