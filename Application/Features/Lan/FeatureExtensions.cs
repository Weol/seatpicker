using Marten;
using Marten.Events.Projections;
using Microsoft.Extensions.DependencyInjection;

namespace Seatpicker.Application.Features.Lan;

internal static class FeatureExtension
{
    public static IServiceCollection AddLanFeature(this IServiceCollection services)
    {
        return services.ConfigureMarten(
            options =>
            {
                options.Projections.Add<LanProjection>(ProjectionLifecycle.Inline);
            });
    }
}