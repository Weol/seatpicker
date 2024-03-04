using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features;
using Seatpicker.Application.Features.Lans;
using Seatpicker.Infrastructure.Entrypoints.Http.Guild;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Lan;

#pragma warning disable CS1998
public static class GetLan
{
    public static async Task<IResult> GetAll(
        [FromServices] IDocumentRepository documentRepository)
    {
        using var documentReader = documentRepository.CreateReader();

        var lans = documentReader.Query<ProjectedLan>()
            .OrderByDescending(lan => lan.CreatedAt)
            .AsEnumerable()
            .Select(lan => Response.FromProjectedLan(lan));

        return TypedResults.Ok(lans);
    }

    public static async Task<IResult> Get(
        [FromRoute] string guildId,
        [FromRoute] Guid lanId,
        [FromServices] IDocumentRepository documentRepository)
    {
        using var documentReader = documentRepository.CreateReader();

        var lan = documentReader.Query<ProjectedLan>()
            .SingleOrDefault(lan => lan.Id == lanId);

        if (lan is null) return TypedResults.NotFound();

        return TypedResults.Ok(Response.FromProjectedLan(lan));
    }

    public static async Task<IResult> GetActiveLan(
        [FromRoute] string guildId,
        [FromServices] IDocumentRepository documentRepository)
    {
        using var documentReader = documentRepository.CreateReader();

        var lan = documentReader.Query<ProjectedLan>()
            .Where(lan => lan.GuildId == guildId)
            .SingleOrDefault(lan => lan.Active);

        if (lan is null) return TypedResults.NotFound();

        return TypedResults.Ok(Response.FromProjectedLan(lan));
    }

    public record Response(
        Guid Id,
        string GuildId,
        bool Active,
        string Title,
        byte[] Background,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt)
    {
        public static Response FromProjectedLan(ProjectedLan lan) => new Response(
            lan.Id,
            lan.GuildId,
            lan.Active,
            lan.Title,
            lan.Background,
            lan.CreatedAt,
            lan.UpdatedAt);
    }
}