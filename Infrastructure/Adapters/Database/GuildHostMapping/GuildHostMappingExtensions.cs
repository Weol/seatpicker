using Marten;

namespace Seatpicker.Infrastructure.Adapters.Database.GuildHostMapping;

public static class GuildHostMappingExtensions
{
    public static IServiceCollection AddGuildHostMappingRepository(this IServiceCollection services)
    {
        return services
            .AddSingleton<GuildHostMappingRepository>()
            .ConfigureMarten(options =>
            {
                options.Schema.For<GuildHostMapping>().SingleTenanted();
            });
    }
}