using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Reservation;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Reservation;

public static class DeleteReservation
{
    public static async Task<IResult> Delete(
        [FromRoute] string guildId,
        [FromRoute] string lanId,
        [FromRoute] string seatId,
        [FromServices] ILoggedInUserAccessor loggedInUserAccessor,
        [FromServices] ReservationService reservationService)
    {
        var user = await loggedInUserAccessor.GetUser();

        await reservationService.Remove(seatId, user);

        return TypedResults.Ok();
    }
}