using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Seats;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Reservation;

public static class DeleteReservation
{
    public static async Task<IResult> Delete(
        [FromRoute] string guildId,
        [FromRoute] Guid lanId,
        [FromRoute] Guid seatId,
        [FromServices] ILoggedInUserAccessor loggedInUserAccessor,
        [FromServices] IReservationService reservationService)
    {
        var user = await loggedInUserAccessor.GetUser();

        await reservationService.Remove(lanId, seatId, user);

        return TypedResults.Ok();
    }
}