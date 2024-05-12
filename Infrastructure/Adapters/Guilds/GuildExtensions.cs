using Marten;

namespace Seatpicker.Infrastructure.Adapters.Guilds;

public static class GuildExtensions
{
    public static IServiceCollection AddGuildAdapter(this IServiceCollection services)
    {
        return services
            .AddSingleton<GuildAdapter>()
            .ConfigureMarten(options =>
            {
                options.Schema.For<GuildAdapter.GuildDocument>().SingleTenanted();
            });
    }
}