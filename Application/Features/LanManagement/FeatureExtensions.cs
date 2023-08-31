using Microsoft.Extensions.DependencyInjection;

namespace Seatpicker.Application.Features.LanManagement;

internal static class FeatureExtension
{
    public static IServiceCollection AddLanManagementFeature(this IServiceCollection services)
    {
        return services
            .AddScoped<ILanManagementService, LanManagementManagementService>();
    }
}