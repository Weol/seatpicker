using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Lans;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Lan;

public static class DeleteLan
{
    public static async Task<IResult> Delete(
        [FromRoute] string guildId,
        [FromRoute] Guid lanId,
        ILoggedInUserAccessor loggedInUserAccessor,
        ILanManagementService lanManagementService)
    {
        var user = await loggedInUserAccessor.GetUser();

        await lanManagementService.Delete(lanId, user);

        return TypedResults.Ok();
    }
}