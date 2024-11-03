using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Lan;
using Seatpicker.Infrastructure.Adapters.Database;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Guild;

public static class GetGuild
{
    public static async Task<IResult> GetAll(
        [FromServices] DocumentRepository documentRepository)
    {
        var reader = documentRepository.CreateGuildlessReader();

        var guilds = reader.Query<Application.Features.Lan.Guild>()
            .AsEnumerable()
            .Select(Response.FromGuild)
            .ToArray();

        return TypedResults.Ok(guilds);
    }

    public static async Task<IResult> Get(
        [FromRoute] string guildId,
        [FromServices] DocumentRepository documentRepository)
    {
        var reader = documentRepository.CreateGuildlessReader();

        var guild = reader
            .Query<Application.Features.Lan.Guild>()
            .SingleOrDefault(guild => guild.Id == guildId);

        if (guild is null) return TypedResults.NotFound();

        return TypedResults.Ok(Response.FromGuild(guild));
    }

    public record Response(
        string Id,
        string Name,
        string? Icon,
        IEnumerable<string> Hostnames,
        GuildRoleMapping[] RoleMapping)
    {
        public static Response FromGuild(Application.Features.Lan.Guild guild)
        {
            return new Response(guild.Id, guild.Name, guild.Icon, guild.Hostnames, guild.RoleMapping);
        }
    }
}