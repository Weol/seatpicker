using Marten;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Lan;
using Seatpicker.Infrastructure.Adapters.Database;
using Seatpicker.Infrastructure.Adapters.Discord;
using Seatpicker.Infrastructure.Entrypoints.Http.Lan;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Frontend;

public static class Discover
{
    public static async Task<IResult> Get(
        [FromHeader(Name = "Host")] string? host,
        [FromServices] DocumentRepository documentRepository,
        [FromServices] DiscordAdapter discordAdapter)
    {
        if (host is null) return Results.BadRequest("Host header cannot be null");

        await using var guildlessReader = documentRepository.CreateGuildlessReader();

        var guild = guildlessReader
            .Query<Application.Features.Lan.Guild>()
            .SingleOrDefault(guild => guild.Hostnames.Contains(host));

        if (guild is null) return Results.NotFound();

        await using var guildReader = documentRepository.CreateReader(guild.Id);

        var activeLan = guildReader
            .Query<ProjectedLan>()
            .FirstOrDefault(lan => lan.Active);

        var lanResponse = activeLan is null ? null : LanResponse.FromProjectedLan(activeLan);

        return TypedResults.Ok(new Response(guild.Id, lanResponse));
    }

    public record Response(string GuildId, LanResponse? Lan);
}