using Microsoft.AspNetCore.Mvc;
using Seatpicker.Infrastructure.Adapters.Discord;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Guild;

public static class GetUnconfiguredGuild
{
    public static async Task<IResult> GetAll(
        [FromServices] DiscordAdapter discordAdapter)
    {
        var guilds = (await discordAdapter.GetGuilds())
            .Select(Response.FromGuild);

        return TypedResults.Ok(guilds);
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