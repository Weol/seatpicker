using Microsoft.AspNetCore.Mvc;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Reservation;

public partial class ReservationController
{
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remove([FromRoute] Guid id)
    {
        var user = loggedInUserAccessor.Get();

        await reservationService.Remove(id, user);

        return new OkResult();
    }
}