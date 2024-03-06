using Marten;

namespace Seatpicker.Infrastructure.Adapters.Database.GuildRoleMapping;

public static class GuildRoleMappingExtensions
{
    public static IServiceCollection AddGuildRoleMappingRepository(this IServiceCollection services)
    {
        return services
            .AddSingleton<GuildRoleMappingRepository>();
    }
}