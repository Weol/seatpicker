using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Infrastructure.Authentication.Discord.DiscordClient;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Discord.Guild;

[ApiController]
[Route("api/discord/guilds")]
[Area("discord")]
[Authorize(Roles = "Admin")]
public class GetEndpoint
{
    [HttpGet("")]
    public async Task<ActionResult<Response[]>> GetAll(
        [FromServices] DiscordClient discordClient)
    {
        var guilds = (await discordClient.GetGuilds())
            .Select(guild => new Response(guild.Id, guild.Name, guild.Icon));

        return new OkObjectResult(guilds);
    }

    public record Response(string Id, string Name, string? Icon);
}