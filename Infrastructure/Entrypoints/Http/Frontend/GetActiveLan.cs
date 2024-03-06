using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features;
using Seatpicker.Application.Features.Lans;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Frontend;

public static class GetLan
{
    public static async Task<IResult> Get(
        [FromHeader(Name = "Host")] string? host,
        [FromServices] IDocumentRepository documentRepository)
    {
        if (host is null) return Results.BadRequest("Host header cannot be null");
        
        using var documentReader = documentRepository.CreateReader();

         return TypedResults.Ok();
    }

    public record Response(string GuildId, GetLan.Response Lan);
}