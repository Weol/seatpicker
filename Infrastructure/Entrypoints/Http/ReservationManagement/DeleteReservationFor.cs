using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Reservation;

namespace Seatpicker.Infrastructure.Entrypoints.Http.ReservationManagement;

public static class DeleteReservationFor
{
    public static async Task<IResult> Delete(
        [FromRoute] string guildId,
        [FromRoute] string lanId,
        [FromRoute] string seatId,
        [FromServices] ILoggedInUserAccessor loggedInUserAccessor,
        [FromServices] ReservationManagementService reservationManagementService)
    {
        var user = await loggedInUserAccessor.GetUser();

        await reservationManagementService.Delete(lanId, seatId, user);

        return TypedResults.Ok();
    }
}