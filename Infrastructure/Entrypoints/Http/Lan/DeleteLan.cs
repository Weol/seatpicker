using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Lan;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Lan;

public static class DeleteLan
{
    public static async Task<IResult> Delete(
        [FromRoute] string guildId,
        [FromRoute] string lanId,
        [FromServices] ILoggedInUserAccessor loggedInUserAccessor,
        [FromServices] LanService lanService)
    {
        var user = await loggedInUserAccessor.GetUser();

        await lanService.Delete(lanId, user);

        return TypedResults.Ok();
    }
}