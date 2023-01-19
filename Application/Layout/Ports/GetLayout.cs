namespace Seatpicker.SeatContext.Layout.Ports;

public interface ILookupTables
{
    Task<IEnumerable<Table>> Get(Guid id);
}
