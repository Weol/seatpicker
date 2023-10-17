using Marten;
using Marten.Events.Projections;
using Microsoft.Extensions.DependencyInjection;
using Seatpicker.Application.Features.Seats;

namespace Seatpicker.Application.Features.Lans;

internal static class FeatureExtension
{
    public static IServiceCollection AddLanManagementFeature(this IServiceCollection services)
    {
        services.ConfigureMarten(
            options =>
            {
                options.Projections.Add<LanProjection>(ProjectionLifecycle.Inline);
            });

        return services
            .AddScoped<ILanManagementService, LanManagementManagementService>();
    }
}