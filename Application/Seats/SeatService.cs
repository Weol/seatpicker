using Microsoft.Extensions.DependencyInjection;
using Seatpicker.SeatContext.Layout;
using Seatpicker.SeatContext.Seats.Ports;

namespace Seatpicker.SeatContext.Seats;

public interface ISeatService
{
    Task<IEnumerable<Seat>> GetAll();
}

public class SeatService : ISeatService
{
    private readonly IGetOccupiedTables getOccupiedTables;
    private readonly ILayoutService layoutService;

    public SeatService(IGetOccupiedTables getOccupiedTables, ILayoutService layoutService)
    {
        this.getOccupiedTables = getOccupiedTables;
        this.layoutService = layoutService;
    }

    public async Task<IEnumerable<Seat>> GetAll()
    {
        var (occupiedTables, layout) = await WhenBoth(getOccupiedTables.Get(), layoutService.GetActiveLayout());

        User? getUser(Guid tableId)
        {
            foreach (var (id, user) in occupiedTables)
            {
                if (tableId == id) return user;
            }

            return null;
        }

        return layout.Tables.Select(table => new Seat(table, getUser(table.Id))).ToArray();
    }
    
    private async Task<(T1, T2)> WhenBoth<T1, T2>(Task<T1> task1, Task<T2> task2)
    {
        await Task.WhenAll(task1, task2);

        return (await task1, await task2);
    }
}

public static class SeatServiceExtensions
{
    public static IServiceCollection AddSeatService(this IServiceCollection services)
    {
        return services.AddScoped<ISeatService, SeatService>();
    }
}
