using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Infrastructure.Authentication;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Guild;

public static class GetUsers
{
    public static async Task<IResult> Get(
        [FromServices] UserManager userManager)
    {
        var users = (await userManager.GetAll())
            .Select(User.FromDomainUser);

        return TypedResults.Ok(users);
    }
}