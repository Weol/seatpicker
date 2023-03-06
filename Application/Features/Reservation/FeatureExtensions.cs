using Microsoft.Extensions.DependencyInjection;

namespace Seatpicker.Application.Features.Reservation;

internal static class Feature
{
    public static IServiceCollection AddReservationFeature(this IServiceCollection services)
    {
        return services
            .AddSingleton<IReservationService, ReservationService>();
    }
}