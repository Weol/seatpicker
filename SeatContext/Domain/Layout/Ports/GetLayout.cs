namespace Seatpicker.SeatContext.Domain.Layout.Ports;

public interface ILookupTables
{
    Task<IEnumerable<Table>> Get(Guid id);
}
