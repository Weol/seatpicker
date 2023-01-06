using Microsoft.Extensions.DependencyInjection;

namespace Seatpicker.SeatContext;

public static class SeatContextExtensions
{
    public static IServiceCollection AddSeatContext(this IServiceCollection services)
    {
        return services;
    }
}