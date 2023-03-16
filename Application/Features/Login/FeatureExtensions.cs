using Microsoft.Extensions.DependencyInjection;

namespace Seatpicker.Application.Features.Login;

internal static class FeatureExtension
{
    public static IServiceCollection AddLoginFeature(this IServiceCollection services)
    {
        return services.AddSingleton<ILoginService, LoginService>();
    }
}