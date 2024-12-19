using Microsoft.AspNetCore.Mvc;
using Seatpicker.Infrastructure.Adapters.Database;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Guild;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
public static class GetGuild
{
    public static async Task<IResult> GetAll(
        [FromServices] DocumentRepository documentRepository)
    {
        await using var reader = documentRepository.CreateGuildlessReader();

        var guilds = reader.Query<Application.Features.Lan.Guild>()
            .AsEnumerable()
            .ToArray();

        return TypedResults.Ok(guilds);
    }

    public static async Task<IResult> Get(
        [FromRoute] string guildId,
        [FromServices] DocumentRepository documentRepository)
    {
        await using var reader = documentRepository.CreateGuildlessReader();

        var guild = reader
            .Query<Application.Features.Lan.Guild>()
            .SingleOrDefault(guild => guild.Id == guildId);

        if (guild is null) return TypedResults.NotFound();

        return TypedResults.Ok(guild);
    }
}