using Seatpicker.Domain;
using Seatpicker.Infrastructure.Entrypoints.Http.Authentication;
using Seatpicker.Infrastructure.Entrypoints.Http.Authentication.Discord;
using Seatpicker.Infrastructure.Entrypoints.Http.Marten;

namespace Seatpicker.Infrastructure.Entrypoints.Http;

public static class EntrypointsMappingExtensions
{
    public static TBuilder RequireRole<TBuilder>(this TBuilder builder, Role role)
        where TBuilder : IEndpointConventionBuilder
    {
        return builder.RequireAuthorization(b => b.RequireRole(role.ToString()));
    }

    public static void MapEntrypoints(this IEndpointRouteBuilder builder)
    {
        var rootBuilder = builder.MapGroup("");

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
        builder.MapGet("test/{guildId}", TestEndpoint.Test);

        var discordGroup = builder.MapGroup("discord");
        discordGroup.MapPost("login", LoginEndpoint.Login);
        discordGroup.MapPost("renew", RenewEndpoint.Renew);
    }
}