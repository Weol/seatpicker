using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Seats;
using Seatpicker.Infrastructure.Entrypoints.Http.Utils;

namespace Seatpicker.Infrastructure.Entrypoints.Http.ReservationManagement;

[ApiController]
[Route("reservationmanagement")]
public class Remove
{
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Endpoint(
        [FromRoute] Guid id,
        [FromServices] ILoggedInUserAccessor loggedInUserAccessor,
        [FromServices] IReservationManagementService reservationManagementService)
    {
        var user = await loggedInUserAccessor.Get();

        await reservationManagementService.Remove(id, user);

        return new OkResult();
    }
}