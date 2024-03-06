using Microsoft.AspNetCore.Mvc;
using Seatpicker.Infrastructure.Adapters.Database.GuildHostMapping;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Guild;

public static class GetHostMapping
{
    public static async Task<IResult> GetALl(
        [FromServices] GuildHostMappingRepository guildHostMappingRepository)
    {
        var response = await guildHostMappingRepository.GetAll()
            .Select(mapping => new Response(mapping.GuildId, mapping.Hostnames))
            .ToArrayAsync();
        
        return Results.Ok(response);
    }

    public record Response(string GuildId, IEnumerable<string> Hostnames);
}