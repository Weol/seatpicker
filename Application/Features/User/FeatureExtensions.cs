using Microsoft.Extensions.DependencyInjection;

namespace Seatpicker.Application.Features.Token;

internal static class FeatureExtension
{
    public static IServiceCollection AddLoginFeature(this IServiceCollection services)
    {
        return services.AddSingleton<ILoginService, TokenService>();
    }
}