using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Infrastructure.Authentication.Discord.DiscordClient;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Guild;

[ApiController]
[Route("api/guild/{guildId}/roles")]
[Area("guilds")]
[Authorize(Roles = "Admin")]
public class GetRolesEndpoint
{
    [HttpGet("")]
    public async Task<ActionResult<Response[]>> GetRoles(
        [FromRoute] string guildId,
        [FromServices] DiscordClient discordClient)
    {
        var roles = (await discordClient.GetGuildRoles(guildId))
            .Select(role => new Response(role.Id, role.Name, role.Color));

        return new OkObjectResult(roles);
    }
    
    public record Response(string Id, string Name, int Color);
}