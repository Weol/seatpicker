using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features;
using Seatpicker.Application.Features.Lans;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Lan;

#pragma warning disable CS1998
public static class GetLan
{
    public static async Task<ActionResult<Response[]>> GetAll(
        [FromServices] IDocumentRepository documentRepository)
    {
        using var documentReader = documentRepository.CreateReader();

        var lans = documentReader.Query<ProjectedLan>()
            .OrderByDescending(lan => lan.CreatedAt)
            .AsEnumerable()
            .Select(lan => new Response(lan));

        return new OkObjectResult(lans);
    }

    public static async Task<ActionResult<Response>> Get(
        [FromRoute] Guid id,
        [FromServices] IDocumentRepository documentRepository)
    {
        using var documentReader = documentRepository.CreateReader();

        var lan = documentReader.Query<ProjectedLan>()
            .SingleOrDefault(lan => lan.Id == id);

        if (lan is null) return new NotFoundResult();

        return new OkObjectResult(new Response(lan));
    }

    public static async Task<ActionResult<Response>> GetActiveLan(
        [FromQuery] string guildId,
        [FromServices] IDocumentRepository documentRepository)
    {
        using var documentReader = documentRepository.CreateReader();

        var lan = documentReader.Query<ProjectedLan>()
            .Where(lan => lan.GuildId == guildId)
            .SingleOrDefault(lan => lan.Active);

        if (lan is null) return new NotFoundResult();

        return new OkObjectResult(new Response(lan));
    }

    public record Response(Guid Id,
        string GuildId,
        bool Active,
        string Title,
        byte[] Background,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt)
    {
        public Response(ProjectedLan lan) : this(
            lan.Id,
            lan.GuildId,
            lan.Active,
            lan.Title,
            lan.Background,
            lan.CreatedAt,
            lan.UpdatedAt)
        {
        }
    }
}