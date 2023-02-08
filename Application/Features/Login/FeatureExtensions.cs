using Microsoft.Extensions.DependencyInjection;

namespace Seatpicker.Application.Features.Login;

internal static class Feature
{
    public static IServiceCollection AddLoginFeature(this IServiceCollection services)
    {
        return services
            .AddSingleton<ILoginService, LoginService>()
            .AddSingleton<IJwtTokenService, JwtTokenService>();
    }
}