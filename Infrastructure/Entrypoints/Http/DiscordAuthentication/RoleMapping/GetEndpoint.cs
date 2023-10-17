using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Infrastructure.Authentication;
using Seatpicker.Infrastructure.Authentication.Discord;

namespace Seatpicker.Infrastructure.Entrypoints.Http.DiscordAuthentication.RoleMapping;

[ApiController]
[Route("authentication/discord")]
[Area("discordAuthentication")]
[Authorize(Roles = "Admin")]
public class GetEndpoint
{
    [HttpGet("roles/{guildId}")]
    public Task<ActionResult<IEnumerable<Response>>> Set(
        [FromServices] DiscordAuthenticationService discordAuthenticationService,
        [FromRoute] string guildId)
    {
        var response = discordAuthenticationService.GetRoleMapping(guildId)
            .Select(tuple => new Response(tuple.RoleId, tuple.RoleName, tuple.Color, tuple.Icon, tuple.Role));

        return Task.FromResult<ActionResult<IEnumerable<Response>>>(new OkObjectResult(response));
    }

    public record Response(
        string DiscordRoleId,
        string DiscordRoleName,
        int DiscordRoleColor,
        string? DiscordRoleIcon,
        Role? Role);
}