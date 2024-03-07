using Microsoft.AspNetCore.Mvc;
using Seatpicker.Infrastructure.Adapters.Database.GuildHostMapping;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Guild;

public static class DiscoverGuild
{
    public static async Task<IResult> Get(
        [FromHeader(Name = "Host")] string? host,
        [FromServices] GuildHostMappingRepository hostMappingRepository)
    {
        if (host is null) return Results.BadRequest("Host header cannot be null");

        var guildId = await hostMappingRepository.GetGuildIdByHost(host);
        if (guildId is null) return Results.NotFound();
        
        return TypedResults.Ok(new Response(guildId));
    }

    public record Response(string GuildId);
}