using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features;
using Seatpicker.Application.Features.Lans;
using Seatpicker.Infrastructure.Adapters.Discord;
using Seatpicker.Infrastructure.Adapters.Guilds;
using Seatpicker.Infrastructure.Entrypoints.Http.Lan;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Frontend;

public static class Discover
{
    public static async Task<IResult> Get(
        [FromHeader(Name = "Host")] string? host,
        [FromServices] IDocumentRepository documentRepository,
        [FromServices] DiscordAdapter discordAdapter,
        [FromServices] GuildAdapter guildAdapter)
    {
        if (host is null) return Results.BadRequest("Host header cannot be null");

        var guildId = await guildAdapter.GetGuildIdByHost(host);
        if (guildId is null) return Results.NotFound();

        using var reader = documentRepository.CreateReader(guildId);

        var activeLan = reader
            .Query<ProjectedLan>()
            .FirstOrDefault(lan => lan.Active);

        var lanResponse = activeLan is null ? null : LanResponse.FromProjectedLan(activeLan);

        return TypedResults.Ok(new Response(guildId, lanResponse));
    }

    public record Response(string GuildId, LanResponse? Lan);
}