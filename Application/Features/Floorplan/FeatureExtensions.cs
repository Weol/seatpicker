using Microsoft.Extensions.DependencyInjection;

namespace Seatpicker.Application.Features.Floorplan;

internal static class FeatureExtension
{
    public static IServiceCollection AddFloorplanFeature(this IServiceCollection services)
    {
        return services
            .AddSingleton<IFloorplanService, FloorplanService>();
    }
}