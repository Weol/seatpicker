using Microsoft.AspNetCore.Mvc;
using Seatpicker.Infrastructure.Adapters.Discord;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Guild;

public static class GetGuilds
{
    public static async Task<IResult> GetAll(
        [FromServices] DiscordAdapter discordAdapter)
    {
        var guilds = (await discordAdapter.GetGuilds())
            .Select(guild => new Response(guild.Id, guild.Name, guild.Icon));

        return TypedResults.Ok(guilds);
    }

    public static async Task<IResult> Get(
        [FromServices] DiscordAdapter discordAdapter,
        [FromRoute] string guildId)
    {
        var guild = (await discordAdapter.GetGuilds())
            .FirstOrDefault(guild => guild.Id == guildId);

        if (guild is null) return TypedResults.NotFound();

        return TypedResults.Ok(new Response(guild.Id, guild.Name, guild.Icon));
    }

    public record Response(string Id, string Name, string? Icon);
}