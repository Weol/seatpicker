using Microsoft.AspNetCore.Mvc;
using Seatpicker.Infrastructure.Authentication.Discord.DiscordClient;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Guild;

[ApiController]
[Route("guild")]
[Area("guilds")]
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

    [HttpGet("{guildId}")]
    public async Task<ActionResult<Response[]>> Get(
        [FromServices] DiscordClient discordClient,
        [FromRoute] string guildId)
    {
        var guild = (await discordClient.GetGuilds())
            .FirstOrDefault(guild => guild.Id == guildId);

        if (guild is null) return new NotFoundResult();

        return new OkObjectResult(new Response(guild.Id, guild.Name, guild.Icon));
    }

    public record Response(string Id, string Name, string? Icon);
}