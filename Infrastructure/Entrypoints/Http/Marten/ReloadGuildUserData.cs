using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Infrastructure.Authentication.Discord;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Marten;

[ApiController]
[Route("marten")]
[Authorize(Roles = "Admin")]
public class ReloadGuildUserData
{
    [HttpPost("reloadGuildUsers/{guildId}")]
    [ProducesResponseType(200)]
    public async Task<ActionResult> Rebuild(
        [FromRoute] string guildId,
        [FromServices] DiscordAuthenticationService discordAuthenticationService)
    {
        await discordAuthenticationService.ReloadUserDataFromGuild(guildId);

        return new OkResult();
    }
}