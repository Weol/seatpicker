namespace Seatpicker.SeatContext.Domain.Seats.Ports;

public interface IGetOccupiedTables
{
    Task<IEnumerable<(Guid TableId, User User)>> Get();
}