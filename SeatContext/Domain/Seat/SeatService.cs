namespace Seatpicker.SeatContext.Domain.Seat;

public interface ISeatService
{
    Task<Seat[]> GetAll();
}

public class SeatService : ISeatService
{
    private readonly IGetSeatConfiguration seatConfiguration;
    private readonly IGetOccupiedSeats occupiedSeatsService;
        
    public async Task<Seat[]> GetAll()
    {
        
    }
}