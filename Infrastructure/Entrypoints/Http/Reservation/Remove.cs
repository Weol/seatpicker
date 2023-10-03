using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Seats;
using Seatpicker.Infrastructure.Entrypoints.Http.Utils;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Reservation;

[ApiController]
[Route("reservation")]
public class Remove
{
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Endpoint(
        [FromRoute] Guid id,
        [FromServices] ILoggedInUserAccessor loggedInUserAccessor,
        [FromServices] IReservationService reservationService)
    {
        var user = await loggedInUserAccessor.Get();

        await reservationService.Remove(id, user);

        return new OkResult();
    }
}