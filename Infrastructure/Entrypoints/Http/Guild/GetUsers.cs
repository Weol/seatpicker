using Microsoft.AspNetCore.Mvc;
using Seatpicker.Infrastructure.Authentication;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Guild;

public static class GetUsers
{
    public static async Task<IResult> Get(
        [FromRoute] string guildId,
        [FromServices] UserManager userManager)
    {
        var users = (await userManager.GetAll(guildId))
            .Select(User.FromDomainUser);

        return TypedResults.Ok(users);
    }
}