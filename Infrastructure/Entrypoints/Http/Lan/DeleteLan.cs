using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Lans;
using Seatpicker.Infrastructure.Entrypoints.Utils;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Lan;

public static class DeleteLan
{
    public static async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        ILoggedInUserAccessor loggedInUserAccessor,
        ILanManagementService lanManagementService)
    {
        var user = await loggedInUserAccessor.Get();

        await lanManagementService.Delete(id, user);

        return new OkResult();
    }
}