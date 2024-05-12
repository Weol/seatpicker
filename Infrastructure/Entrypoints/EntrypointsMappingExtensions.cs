using Seatpicker.Domain;
using Seatpicker.Infrastructure.Entrypoints.Filters;
using Seatpicker.Infrastructure.Entrypoints.Http.Authentication;
using Seatpicker.Infrastructure.Entrypoints.Http.Authentication.Discord;
using Seatpicker.Infrastructure.Entrypoints.Http.Frontend;
using Seatpicker.Infrastructure.Entrypoints.Http.Guild;
using Seatpicker.Infrastructure.Entrypoints.Http.Lan;
using Seatpicker.Infrastructure.Entrypoints.Http.Marten;
using Seatpicker.Infrastructure.Entrypoints.Http.Reservation;
using Seatpicker.Infrastructure.Entrypoints.Http.ReservationManagement;
using Seatpicker.Infrastructure.Entrypoints.Http.Seat;
using UpdateLan = Seatpicker.Infrastructure.Entrypoints.Http.Lan.UpdateLan;

namespace Seatpicker.Infrastructure.Entrypoints;

public static class EntrypointsMappingExtensions
{
    public static TBuilder RequireRole<TBuilder>(this TBuilder builder, Role role) where TBuilder : IEndpointConventionBuilder
    {
        return builder.RequireAuthorization(b => b.RequireRole(role.ToString()));
    }

    public static void MapEntrypoints(this IEndpointRouteBuilder builder, Action<RouteGroupBuilder> rootRouteBuilder)
    {
        var rootBuilder = builder.MapGroup("");
        rootRouteBuilder(rootBuilder);

        rootBuilder.MapGroup("guild")
            .MapGuildEndpoints();

        rootBuilder.MapGroup("authentication")
            .MapAuthenticationEndpoints();

        rootBuilder.MapGroup("marten")
            .MapMartenEndpoints();

        rootBuilder.MapMartenEndpoints();
    }

    private static void MapMartenEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("rebuildprojections", RebuildProjections.Rebuild);
    }

    private static void MapAuthenticationEndpoints(this RouteGroupBuilder builder)
    {
        builder.MapGet("test/{guildId}", TestEndpoint.Test)
            .RequireAuthorization()
            .AddEndpointFilter<GuildIdAuthorizationFilter>();

        var discordGroup = builder.MapGroup("discord");
        discordGroup.MapPost("login", LoginEndpoint.Login);
        discordGroup.MapPost("renew", RenewEndpoint.Renew);
    }

    private static RouteGroupBuilder MapGuildEndpoints(this RouteGroupBuilder builder)
    {
        builder.MapGet("/", GetGuild.GetAll)
            .RequireRole(Role.Superadmin);

        builder.MapGet("/discover", Discover.Get);

        var guildGroup = builder.MapGroup("{guildId}")
            .AddEndpointFilter<GuildIdAuthorizationFilter>();

        guildGroup.MapPut("/", UpdateGuild.Update)
            .RequireRole(Role.Admin);

        guildGroup.MapGet("/", GetGuild.Get);

        guildGroup.MapGroup("lan")
            .MapLanEndpoints();

        guildGroup.MapGet("users", GetUsers.Get)
            .RequireRole(Role.Operator);

        return builder;
    }

    private static void MapLanEndpoints(this RouteGroupBuilder builder)
    {
        builder.MapGet("/", GetLan.GetAll);
        builder.MapPost("/", CreateLan.Create).RequireRole(Role.Admin);

        var lanGroup = builder.MapGroup("{lanId:Guid}");
        lanGroup.MapGet("/", GetLan.Get).RequireRole(Role.Admin);
        lanGroup.MapPut("/", UpdateLan.Update).RequireRole(Role.Admin);
        lanGroup.MapDelete("/", DeleteLan.Delete).RequireRole(Role.Admin);

        lanGroup.MapGroup("seat")
            .MapSeatEndpoints();
    }

    private static void MapSeatEndpoints(this RouteGroupBuilder builder)
    {
        builder.MapGet("/", GetSeat.GetAll);
        builder.MapPost("/", CreateSeat.Create).RequireRole(Role.Operator);

        var seatGroup = builder.MapGroup("{seatId:Guid}");
        seatGroup.MapGet("/", GetSeat.Get).RequireRole(Role.Operator);
        seatGroup.MapPut("/", UpdateSeat.Update).RequireRole(Role.Operator);
        seatGroup.MapDelete("/", DeleteSeat.Delete).RequireRole(Role.Operator);

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