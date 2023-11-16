using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Infrastructure.Authentication;
using Seatpicker.Infrastructure.Authentication.Discord;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Discord.Guild.RoleMapping;

[ApiController]
[Route("api/discord/guilds/{guildId}/roles")]
[Area("discord")]
[Authorize(Roles = "Admin")]
public class SetEndpoint
{
    [HttpPut("")]
    public async Task<IActionResult> Set(
        [FromServices] DiscordAuthenticationService discordAuthenticationService,
        [FromBody] Request request,
        [FromRoute] string guildId)
    {
        await discordAuthenticationService.SetRoleMapping(guildId, request.RoleMappings);
        return new OkResult();
    }

    public record Request(IEnumerable<(string RoleId, Role Role)> RoleMappings);
}