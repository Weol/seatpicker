using Seatpicker.Domain;

namespace Seatpicker.Application.Features.Floorplan;

public interface IFloorplanService
{
    public Task UpdateSeats(ICollection<UpdateSeat> seatUpdates);
}

internal class FloorplanService : IFloorplanService
{
    private readonly ISeatRepository seatRepository;

    public FloorplanService(ISeatRepository seatRepository)
    {
        this.seatRepository = seatRepository;
    }

    public async Task UpdateSeats(ICollection<UpdateSeat> seatUpdates)
    {
        var seats = await seatRepository.GetAll();

        var tasks = seatUpdates.Select(
            updateSeat =>
            {
                var seat = seats.FirstOrDefault(seat => seat.Id == updateSeat.Id) ?? new Seat
                {
                    Id = updateSeat.Id,
                };

                seat.X = updateSeat.X;
                seat.Y = updateSeat.Y;
                seat.Width = updateSeat.Width;
                seat.Height = updateSeat.Height;
                seat.Title = updateSeat.Title;

                return seatRepository.Store(seat);
            }).ToArray();

        Task.WaitAll(tasks);
    }
}

public record UpdateSeat(Guid Id, string Title, double X, double Y, double Width, double Height);