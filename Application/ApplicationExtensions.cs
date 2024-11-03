using Microsoft.Extensions.DependencyInjection;
using Seatpicker.Application.Features.Lan;
using Seatpicker.Application.Features.Reservation;

namespace Seatpicker.Application;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services
            .AddLanFeature()
            .AddReservationFeature();
    }
}