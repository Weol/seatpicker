using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Infrastructure.Authentication;
using Seatpicker.Infrastructure.Authentication.Discord;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Discord.Guild.RoleMapping;

[ApiController]
[Route("api/discord/guild/{guildId}/roles")]
[Area("discord")]
[Authorize(Roles = "Admin")]
public class GetEndpoint
{
    [HttpGet("")]
    public Task<ActionResult<IEnumerable<Response>>> Set(
        [FromServices] DiscordAuthenticationService discordAuthenticationService,
        [FromRoute] string guildId)
    {
        var response = discordAuthenticationService.GetRoleMapping(guildId)
            .Select(tuple => new Response(tuple.RoleId, tuple.RoleName, tuple.Color, tuple.Icon, tuple.Role));

        return Task.FromResult<ActionResult<IEnumerable<Response>>>(new OkObjectResult(response));
    }

    public record Response(
        string RoleId,
        string RoleName,
        int RoleColor,
        string? RoleIcon,
        Role? Role);
}