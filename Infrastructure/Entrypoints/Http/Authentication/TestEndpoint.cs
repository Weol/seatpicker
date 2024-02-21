using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Infrastructure.Entrypoints.Utils;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Authentication;

public static class TestEndpoint
{
    public static async Task<ActionResult<Response>> Test(
        [FromServices] ILoggedInUserAccessor loggedInUserAccessor,
        [FromServices] IHttpContextAccessor httpContextAccessor)
    {
        var httpContext = httpContextAccessor.HttpContext ?? throw new NullReferenceException();
        var roles = httpContext.User.Identities.SelectMany(
                identity => identity.Claims.Where(claim => claim.Type == identity.RoleClaimType)
                    .Select(claim => claim.Value))
            .ToArray();

        var loggedInUser = await loggedInUserAccessor.Get();

        return new OkObjectResult(new Response(loggedInUser.Id, loggedInUser.Name, roles));
    }

    public record Response(string? Id, string? Name, string[] Roles);
}