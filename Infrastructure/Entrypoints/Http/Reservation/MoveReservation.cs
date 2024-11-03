using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Reservation;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Reservation;

public static class MoveReservation
{
    public static async Task<IResult> Move(
        [FromRoute] string guildId,
        [FromRoute] string lanId,
        [FromRoute] string seatId,
        [FromBody] Request request,
        [FromServices] ILoggedInUserAccessor loggedInUserAccessor,
        [FromServices] ReservationService reservationService)
    {
        var user = await loggedInUserAccessor.GetUser();

        await reservationService.Move(seatId, request.ToSeatId, user);

        return TypedResults.Ok();
    }

    public record Request(string ToSeatId);

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.ToSeatId).NotEmpty();
        }
    }
}