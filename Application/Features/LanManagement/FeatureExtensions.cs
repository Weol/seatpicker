using Microsoft.Extensions.DependencyInjection;

namespace Seatpicker.Application.Features.Lan;

internal static class FeatureExtension
{
    public static IServiceCollection AddLanManagementFeature(this IServiceCollection services)
    {
        return services
            .AddSingleton<ILanManagementService, LanManagementManagementService>();
    }
}