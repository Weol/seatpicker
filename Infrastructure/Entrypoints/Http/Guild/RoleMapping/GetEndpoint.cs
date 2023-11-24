using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Infrastructure.Authentication;
using Seatpicker.Infrastructure.Authentication.Discord;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Guild.RoleMapping;

[ApiController]
[Route("api/guilds/{guildId}/roles/mapping")]
[Area("guilds")]
[Authorize(Roles = "Admin")]
public class GetEndpoint
{
    [HttpGet("")]
    public Task<ActionResult<IEnumerable<Response>>> Set(
        [FromServices] DiscordAuthenticationService discordAuthenticationService,
        [FromRoute] string guildId)
    {
        var response = discordAuthenticationService.GetRoleMapping(guildId)
            .Select(mapping => new Response(mapping.RoleId, mapping.Role));

        return Task.FromResult<ActionResult<IEnumerable<Response>>>(new OkObjectResult(response));
    }

    public record Response(
        string RoleId,
        Role? Role);
}