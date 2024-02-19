using System.Net;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Adapters.Database.GuildRoleMapping;
using Seatpicker.Infrastructure.Adapters.Discord;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Guild.Discord;

public static class GetRoleMapping
{
    public static async Task<IResult> Get(
        [FromRoute] string guildId,
        [FromServices] GuildRoleMappingRepository guildRoleRepository,
        [FromServices] DiscordAdapter discordAdapter)
    {
        try
        {
            var guildRolesTask = discordAdapter.GetGuildRoles(guildId);
            var mappingsTask = guildRoleRepository.GetRoleMapping(guildId).ToArrayAsync();

            var guildRoles = await guildRolesTask;
            var mappings = await mappingsTask;

            var responses = guildRoles
                .Select(guildRole => new Response(
                    guildRole.Id,
                    guildRole.Name,
                    guildRole.Color,
                    mappings
                        .Where(mapping => mapping.RoleId == guildRole.Id)
                        .Select(mapping => mapping.Role)));

            return TypedResults.Ok(responses);
        }
        catch (DiscordException e) when (e.StatusCode == HttpStatusCode.NotFound)
        {
            return TypedResults.NotFound();
        }
    }

    public record Response(string Id, string Name, int Color, IEnumerable<Role> Roles);
}