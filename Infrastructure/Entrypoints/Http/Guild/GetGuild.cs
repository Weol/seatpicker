using Microsoft.AspNetCore.Mvc;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Adapters.Guilds;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Guild;

public static class GetGuild
{
    public static async Task<IResult> GetAll(
        [FromServices] GuildAdapter guildAdapter)
    {
        var guilds = await guildAdapter.GetGuilds()
            .Select(Response.FromGuild)
            .ToArrayAsync();

        return TypedResults.Ok(guilds);
    }

    public static async Task<IResult> Get(
        [FromServices] GuildAdapter guildAdapter,
        [FromRoute] string guildId)
    {
        var guild = await guildAdapter.GetGuild(guildId);

        if (guild is null) return TypedResults.NotFound();

        return TypedResults.Ok(Response.FromGuild(guild));
    }

    public record Response(
        string Id,
        string Name,
        string? Icon,
        IEnumerable<string> Hostnames,
        (string GuildRoleId, Role[] Roles)[] RoleMapping,
        GuildRole[] Roles)
    {
        public static Response FromGuild(Adapters.Guilds.Guild guild)
        {
            return new Response(guild.Id, guild.Name, guild.Icon, guild.Hostnames, guild.RoleMapping, guild.Roles);
        }
    }
}