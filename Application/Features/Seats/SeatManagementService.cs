namespace Seatpicker.Application.Features.Seats;

public interface ISeatManagementService
{
    public Task UpdateBounds(Guid seatId, double x, double y, double width, double height);
}

public class SeatManagementService : ISeatManagementService
{
    public Task UpdateBounds(Guid seatId, double x, double y, double width, double height) => throw new NotImplementedException();
}