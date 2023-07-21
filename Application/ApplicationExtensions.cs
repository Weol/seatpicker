using Microsoft.Extensions.DependencyInjection;
using Seatpicker.Application.Features.Lan;
using Seatpicker.Application.Features.Token;

namespace Seatpicker.Application;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services
            .AddLanManagementFeature()
            .AddLoginFeature();
    }
}