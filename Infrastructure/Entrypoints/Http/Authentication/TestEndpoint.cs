using Microsoft.AspNetCore.Mvc;
using Seatpicker.Domain;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Authentication;

public static class TestEndpoint
{
    public static async Task<IResult> Test(
        [FromRoute] string guildId,
        [FromServices] ILoggedInUserAccessor loggedInUserAccessor)
    {
        var loggedInUser = await loggedInUserAccessor.Get();

        return TypedResults.Ok(new Response(loggedInUser.Id, loggedInUser.Name, loggedInUser.Roles));
    }

    public record Response(string? Id, string? Name, IEnumerable<Role> Roles);
    
    
}