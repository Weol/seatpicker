using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features;
using Seatpicker.Infrastructure.Adapters.Discord;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Guild;

public static class GetUnconfiguredGuild
{
    public static async Task<IResult> GetAll(
        [FromServices] IGuildlessDocumentReader documentReader,
        [FromServices] DiscordAdapter discordAdapter)
    {
        var guilds = (await discordAdapter.GetGuilds())
            .Select(Response.FromGuild)
            .ToArray();

        var configuredGuilds = documentReader.Query<Application.Features.Lan.Guild>()
            .ToArray();

        var unconfiguredGuilds = guilds.Where(guild =>
            !configuredGuilds.Any(uncofiguredGuild => uncofiguredGuild.Id == guild.Id));

        return TypedResults.Ok(unconfiguredGuilds);
    }

    public record Response(
        string Id,
        string Name,
        string? Icon)
    {
        public static Response FromGuild(Adapters.Discord.DiscordGuild guild)
        {
            return new Response(guild.Id, guild.Name, guild.Icon);
        }
    }
}