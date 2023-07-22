using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Reservation;

public partial class ReservationController
{
    [HttpDelete("{seatId:guid}")]
    public async Task<IActionResult> Remove([FromRoute] Guid seatId)
    {
        var user = loggedInUserAccessor.Get();

        await reservationService.Remove(seatId, user);

        return new OkResult();
    }
}