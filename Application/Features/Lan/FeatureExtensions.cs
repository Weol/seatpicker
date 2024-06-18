using Marten;
using Marten.Events.Projections;
using Microsoft.Extensions.DependencyInjection;
using Seatpicker.Application.Features.Lans;

namespace Seatpicker.Application.Features.Guilds;

internal static class FeatureExtension
{
    public static IServiceCollection AddGuildFeature(this IServiceCollection services)
    {
        services.ConfigureMarten(
            options =>
            {
                options.Projections.Add<LanProjection>(ProjectionLifecycle.Inline);
            });

        return services
            .AddScoped<LanManagementService>()
            .AddScoped<GuildService>();
    }
}