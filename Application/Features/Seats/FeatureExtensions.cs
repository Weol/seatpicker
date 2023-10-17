using JasperFx.Core;
using Marten;
using Marten.Events.Projections;
using Microsoft.Extensions.DependencyInjection;

namespace Seatpicker.Application.Features.Seats;

internal static class FeatureExtension
{
    public static IServiceCollection AddSeatsFeature(this IServiceCollection services)
    {
        services.ConfigureMarten(
            options =>
            {
                options.Projections.Add<SeatProjection>(ProjectionLifecycle.Inline);
            });

        return services.AddScoped<ISeatManagementService, SeatManagementService>()
            .AddScoped<IReservationManagementService, ReservationManagementService>()
            .AddScoped<IReservationService, ReservationService>();
    }
}