using Microsoft.Extensions.DependencyInjection;
using Seatpicker.SeatContext.Domain.Layout;
using Seatpicker.SeatContext.Domain.Seats;

namespace Seatpicker.SeatContext;

public static class SeatContextExtensions
{
    public static IServiceCollection AddSeatContext(this IServiceCollection services)
    {
        return services
            .AddLayoutService()
            .AddSeatService();
    }
}