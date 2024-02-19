using Seatpicker.Infrastructure.Entrypoints.Http.Guild;
using Seatpicker.Infrastructure.Entrypoints.Http.Guild.Discord;
using Seatpicker.Infrastructure.Entrypoints.Http.Lan;

namespace Seatpicker.Infrastructure.Entrypoints;

public static class EntrypointsMappingExtensions
{
    public static void MapEntrypoints(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("guild")
            .MapGuildEndpoints();
        builder.MapAuthenticationEndpoints();
        builder.MapMartenEndpoints();
    }
    
    private static void MapMartenEndpoints(this IEndpointRouteBuilder builder)
    {
    }
    
    private static void MapAuthenticationEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.
    }

    private static void MapGuildEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("", GetGuilds.GetAll);
        
        var guildBuilder = builder.MapGroup("{guildId}");
        guildBuilder.MapGet("", GetGuilds.Get);

        guildBuilder.MapGet("users", GetUsers.Get);
        
        // Discord guild endpoint
        guildBuilder.MapGet("roles", GetRoleMapping.Get);
        guildBuilder.MapPut("roles", PutRoleMapping.Put);
    }
    
    private static void MapLanEndpoints(this IEndpointRouteBuilder builder)
    {
        builder
    }
    
    private static void MapSeatEndpoints(this IEndpointRouteBuilder builder)
    {
        builder
    }
    
    private static void MapReservationEndpoints(this IEndpointRouteBuilder builder)
    {
        builder
    }
    
    private static void MapReservationManagementEndpoints(this IEndpointRouteBuilder builder)
    {
        builder
    }
}