using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Infrastructure.Authentication;
using Seatpicker.Infrastructure.Authentication.Discord;

namespace Seatpicker.Infrastructure.Entrypoints.Http.DiscordAuthentication.RoleMapping;

[ApiController]
[Route("api/authentication/discord")]
[Area("discordAuthentication")]
[Authorize(Roles = "Admin")]
public class SetEndpoint
{
    [HttpPut("roles/{guildId}")]
    public async Task<IActionResult> Set(
        [FromServices] DiscordAuthenticationService discordAuthenticationService,
        [FromBody] Request request,
        [FromRoute] string guildId)
    {
        await discordAuthenticationService.SetRoleMapping(guildId, request.RoleMappings);
        return new OkResult();
    }

    public record Request(IEnumerable<(string RoleId, Role Role)> RoleMappings);

    public record Response(
        string DiscordRoleId,
        string DiscordRoleName,
        int DiscordRoleColor,
        string? DiscordRoleIcon,
        Role? Role);
}