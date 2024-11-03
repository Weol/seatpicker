using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Lan;
using Seatpicker.Infrastructure.Adapters.Database;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Lan;

#pragma warning disable CS1998
public static class GetLan
{
    public static async Task<IResult> GetAll(
        [FromRoute] string guildId,
        [FromServices] DocumentRepository documentRepository)
    {
        using var documentReader = documentRepository.CreateReader(guildId);

        var lans = documentReader.Query<ProjectedLan>()
            .OrderByDescending(lan => lan.CreatedAt)
            .AsEnumerable()
            .Select(LanResponse.FromProjectedLan);

        return TypedResults.Ok(lans);
    }

    public static async Task<IResult> Get(
        [FromRoute] string guildId,
        [FromRoute] string lanId,
        [FromServices] DocumentRepository documentRepository)
    {
        using var documentReader = documentRepository.CreateReader(guildId);

        var lan = documentReader.Query<ProjectedLan>()
            .SingleOrDefault(lan => lan.Id == lanId);

        if (lan is null) return TypedResults.NotFound();

        return TypedResults.Ok(LanResponse.FromProjectedLan(lan));
    }
}