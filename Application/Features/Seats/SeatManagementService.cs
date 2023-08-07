namespace Seatpicker.Application.Features.Seats;

public interface ISeatManagementService
{
    public Task UpdateBounds(Guid seatId, double x, double y, double width, double height);

    public Task CreateSeat(Guid seatId, double x, double y, double width, double height);

    public Task DeleteSeat(Guid seatId);
}

public class SeatManagementService : ISeatManagementService
{
    public Task UpdateBounds(Guid seatId, double x, double y, double width, double height) => throw new NotImplementedException();

    public Task CreateSeat(Guid seatId, double x, double y, double width, double height) => throw new NotImplementedException();

    public Task DeleteSeat(Guid seatId) => throw new NotImplementedException();
}