using Microsoft.Extensions.DependencyInjection;
using Seatpicker.Application.Features.LanManagement;
using Seatpicker.Domain;

namespace Seatpicker.Application.Features.Seats;

internal static class FeatureExtension
{
    public static IServiceCollection AddSeatsFeature(this IServiceCollection services)
    {
        return services
            .AddScoped<ISeatManagementService, SeatManagementService>()
            .AddScoped<IReservationManagementService, ReservationManagementService>()
            .AddScoped<IReservationService, ReservationService>();
    }
}