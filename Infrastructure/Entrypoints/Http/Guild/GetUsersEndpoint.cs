using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Remotion.Linq.Clauses;
using Seatpicker.Infrastructure.Adapters.Database;
using Seatpicker.Infrastructure.Authentication;
using Seatpicker.Infrastructure.Authentication.Discord.DiscordClient;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Guild;

[ApiController]
[Route("guild/{guildId}")]
[Area("guilds")]
[Authorize(Roles = "Admin")]
public class GetUsersEndpoint
{
    [HttpGet("users")]
    public async Task<ActionResult<User[]>> GetUsers(
        [FromServices] UserManager userManager,
        [FromRoute] string guildId)
    {
        var users = (await userManager.GetAllInGuild(guildId))
            .Select(User.FromDomainUser);

        return new OkObjectResult(users);
    }
}