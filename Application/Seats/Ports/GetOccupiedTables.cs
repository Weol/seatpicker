namespace Seatpicker.SeatContext.Seats.Ports;

public interface IGetOccupiedTables
{
    Task<IEnumerable<(Guid TableId, User User)>> Get();
}