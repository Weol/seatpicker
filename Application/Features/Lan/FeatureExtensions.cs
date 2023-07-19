using Microsoft.Extensions.DependencyInjection;

namespace Seatpicker.Application.Features.Lan;

internal static class FeatureExtension
{
    public static IServiceCollection AddLanFeature(this IServiceCollection services)
    {
        return services
            .AddSingleton<ILanService, LanService>();
    }
}