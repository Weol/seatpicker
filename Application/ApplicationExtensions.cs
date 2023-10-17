using Microsoft.Extensions.DependencyInjection;
using Seatpicker.Application.Features.Lans;
using Seatpicker.Application.Features.Seats;

namespace Seatpicker.Application;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services.AddLanManagementFeature().AddSeatsFeature();
    }
}