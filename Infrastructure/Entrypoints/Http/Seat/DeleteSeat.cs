using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Reservation;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Seat;

public static class DeleteSeat
{
    public static async Task<IResult> Delete(
        [FromRoute] string guildId,
        [FromRoute] string lanId,
        [FromRoute] string seatId,
        [FromServices] ILoggedInUserAccessor loggedInUserAccessor,
        [FromServices] SeatManagementService seatManagementService)
    {
        var user = await loggedInUserAccessor.GetUser();

        await seatManagementService.Remove(seatId, user);

        return TypedResults.Ok();
    }
}