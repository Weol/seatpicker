using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Seats;
using Seatpicker.Infrastructure.Entrypoints.Utils;

namespace Seatpicker.Infrastructure.Entrypoints.Http.ReservationManagement;

[ApiController]
[Route("lan/{lanId:Guid}/seat/{seatId:guid}/reservationmanagement")]
[Authorize(Roles = "Operator")]
public class DeleteEndpoint
{
    [HttpDelete]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid lanId,
        [FromRoute] Guid seatId,
        [FromServices] ILoggedInUserAccessor loggedInUserAccessor,
        [FromServices] IReservationManagementService reservationManagementService)
    {
        var user = await loggedInUserAccessor.Get();

        await reservationManagementService.Delete(lanId, seatId, user);

        return new OkResult();
    }
}