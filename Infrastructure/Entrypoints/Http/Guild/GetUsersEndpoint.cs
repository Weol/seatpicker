using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Infrastructure.Authentication;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Guild;

[ApiController]
[Route("guild/{guildId}")]
[Authorize(Roles = "Operator")]
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