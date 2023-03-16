using Microsoft.Extensions.DependencyInjection;

namespace Seatpicker.Application.Features.Reservation;

internal static class FeatureExtension
{
    public static IServiceCollection AddReservationFeature(this IServiceCollection services)
    {
        return services
            .AddSingleton<IReservationService, ReservationService>();
    }
}