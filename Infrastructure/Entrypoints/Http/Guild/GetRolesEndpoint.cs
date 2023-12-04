using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Infrastructure.Authentication;
using Seatpicker.Infrastructure.Authentication.Discord;
using Seatpicker.Infrastructure.Authentication.Discord.DiscordClient;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Guild;

[ApiController]
[Route("api/guild/{guildId}")]
[Area("guilds")]
[Authorize(Roles = "Admin")]
public class GetRolesEndpoint
{
    [HttpGet("roles")]
    public async Task<ActionResult<Response[]>> GetRoles(
        [FromRoute] string guildId,
        [FromServices] DiscordAuthenticationService discordAuthenticationService,
        [FromServices] DiscordClient discordClient)
    {
        try
        {
            var guildRolesTask = discordClient.GetGuildRoles(guildId);
            var mappingsTask = discordAuthenticationService.GetRoleMapping(guildId).ToArrayAsync();

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

            return new OkObjectResult(responses);
        }
        catch (DiscordException e) when (e.StatusCode == HttpStatusCode.NotFound)
        {
            return new NotFoundResult();
        }
    }

    public record Response(string Id, string Name, int Color, IEnumerable<Role> Roles);
}