using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Seatpicker.Application.Features.Login;

namespace Seatpicker.Application;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services
            .AddLoginFeature();
    }
}