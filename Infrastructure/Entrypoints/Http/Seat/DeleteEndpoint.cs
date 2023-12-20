using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Seats;
using Seatpicker.Infrastructure.Entrypoints.Utils;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Seat;

[ApiController]
[Route("lan/{lanId:guid}/seat")]
[Authorize(Roles = "Operator")]
public class DeleteEndpoint
{
    [HttpDelete("{seatId:guid}")]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid lanId,
        [FromRoute] Guid seatId,
        [FromServices] ILoggedInUserAccessor loggedInUserAccessor,
        [FromServices] ISeatManagementService seatManagementService)
    {
        var user = await loggedInUserAccessor.Get();

        await seatManagementService.Remove(lanId, seatId, user);

        return new OkResult();
    }
}