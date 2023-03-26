using Discord.Rest;
using Microsoft.AspNetCore.Mvc;

namespace Seatpicker.Infrastructure.Entrypoints.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController
{
    private readonly DiscordRestClient client;

    public TestController(DiscordRestClient client)
    {
        this.client = client;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var guilds = await client.GetGuildsAsync();
        foreach (var guild in guilds)
        {
            var asd = await guild.CreateTextChannelAsync("test");
        }


        return new OkResult();
    }
}