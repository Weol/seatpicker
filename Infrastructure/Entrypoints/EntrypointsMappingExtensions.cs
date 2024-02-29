using Seatpicker.Domain;
using Seatpicker.Infrastructure.Entrypoints.Filters;
using Seatpicker.Infrastructure.Entrypoints.Http.Authentication;
using Seatpicker.Infrastructure.Entrypoints.Http.Authentication.Discord;
using Seatpicker.Infrastructure.Entrypoints.Http.Guild;
using Seatpicker.Infrastructure.Entrypoints.Http.Guild.Discord;
using Seatpicker.Infrastructure.Entrypoints.Http.Lan;
using Seatpicker.Infrastructure.Entrypoints.Http.Marten;
using Seatpicker.Infrastructure.Entrypoints.Http.Reservation;
using Seatpicker.Infrastructure.Entrypoints.Http.ReservationManagement;
using Seatpicker.Infrastructure.Entrypoints.Http.Seat;

namespace Seatpicker.Infrastructure.Entrypoints;

public static class EntrypointsMappingExtensions
{
    public static TBuilder RequireRole<TBuilder>(this TBuilder builder, Role role) where TBuilder : IEndpointConventionBuilder
    {
        return builder.RequireAuthorization(b => b.RequireRole(role.ToString()));
    }

    public static void MapEntrypoints(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("guild")
            .MapGuildEndpoints()
            .MapDiscordGuildEndpoints();

        builder.MapGroup("authentication")
            .MapAuthenticationEndpoints();

        builder.MapGroup("marten")
            .MapMartenEndpoints();

        builder.MapMartenEndpoints();
    }

    private static void MapMartenEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("rebuildprojections", RebuildProjections.Rebuild);
    }

    private static void MapAuthenticationEndpoints(this RouteGroupBuilder builder)
    {
        builder.MapPost("test", TestEndpoint.Test)
            .RequireAuthorization();

        var discordGroup = builder.MapGroup("discord");
        discordGroup.MapPost("login", LoginEndpoint.Login);
        discordGroup.MapPost("renew", RenewEndpoint.Renew);
    }

    private static void MapDiscordGuildEndpoints(this RouteGroupBuilder builder)
    {
        var discordGroup = builder.MapGroup("discord/{guildId}")
            .RequireRole(Role.Admin)
            .AddEndpointFilter<GuildIdAuthorizationFilter>();
        
        discordGroup.MapGet("roles", GetRoleMapping.Get);
        discordGroup.MapPut("roles", PutRoleMapping.Put);
    }
    
    private static RouteGroupBuilder MapGuildEndpoints(this RouteGroupBuilder builder)
    {
        builder.MapGet("/", GetGuilds.GetAll);

        var guildGroup = builder.MapGroup("{guildId}")
            .AddEndpointFilter<GuildIdAuthorizationFilter>();
            
        guildGroup.MapGet("/", GetGuilds.Get);

        guildGroup.MapGroup("lan")
            .MapLanEndpoints();
        
        guildGroup.MapGet("users", GetUsers.Get)
            .RequireRole(Role.Admin);

        return builder;
    }

    private static void MapLanEndpoints(this RouteGroupBuilder builder)
    {
        builder.MapGet("/", GetLan.GetAll);
        builder.MapGet("active", GetLan.GetActiveLan);
        builder.MapPost("/", CreateLan.Create).RequireRole(Role.Admin);

        var lanGroup = builder.MapGroup("{lanId:Guid}").RequireRole(Role.Admin);
        lanGroup.MapGet("/", GetLan.Get);
        lanGroup.MapPut("/", UpdateLan.Update);
        lanGroup.MapDelete("/", DeleteLan.Delete);

        lanGroup.MapGroup("seat")
            .MapSeatEndpoints();
    }

    private static void MapSeatEndpoints(this RouteGroupBuilder builder)
    {
        builder.MapGet("/", GetSeat.GetAll);
        builder.MapPost("/", CreateSeat.Create).RequireRole(Role.Operator);

        var seatGroup = builder.MapGroup("{seatId:Guid}").RequireRole(Role.Operator);
        seatGroup.MapGet("/", GetSeat.Get);
        seatGroup.MapPut("/", UpdateSeat.Update);
        seatGroup.MapDelete("/", DeleteSeat.Delete);

        seatGroup.MapGroup("reservation")
            .MapReservationEndpoints();

        seatGroup.MapGroup("reservationmanagement")
            .MapReservationManagementEndpoints();
    }

    private static void MapReservationEndpoints(this RouteGroupBuilder builder)
    {
        builder.MapPost("/", CreateReservation.Create);
        builder.MapPut("/", MoveReservation.Move);
        builder.MapDelete("/", DeleteReservation.Delete);
    }

    private static void MapReservationManagementEndpoints(this RouteGroupBuilder builder)
    {
        builder.RequireRole(Role.Operator);
        builder.MapPost("/", CreateReservationFor.Create);
        builder.MapPut("/", MoveReservationFor.Move);
        builder.MapDelete("/", DeleteReservationFor.Delete);
    }
}