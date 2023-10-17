using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Infrastructure.Entrypoints.Utils;

namespace Seatpicker.Infrastructure.Entrypoints.Http.DiscordAuthentication;

[ApiController]
[Route("authentication/discord")]
[Area("discordAuthentication")]
[Authorize]
public class TestEndpoint
{
    [HttpGet("test")]
    public async Task<ActionResult<Response>> Test(
        [FromServices] LoggedInUserAccessor loggedInUserAccessor,
        [FromServices] HttpContext httpContext)
    {
        var roles = httpContext.User.Identities.SelectMany(
                identity => identity.Claims.Where(claim => claim.Type == identity.RoleClaimType)
                    .Select(claim => claim.Value))
            .ToArray();

        var loggedInUser = await loggedInUserAccessor.Get();

        return new OkObjectResult(new Response(loggedInUser.Id, loggedInUser.Name, roles));
    }

    public record Response(string? Id, string? Name, string[] Roles);
}