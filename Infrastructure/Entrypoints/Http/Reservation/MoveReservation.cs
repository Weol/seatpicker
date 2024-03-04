using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Seats;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Reservation;

public static class MoveReservation
{
    public static async Task<IResult> Move(
        [FromRoute] string guildId,
        [FromRoute] Guid lanId,
        [FromRoute] Guid seatId,
        [FromBody] Request request,
        [FromServices] ILoggedInUserAccessor loggedInUserAccessor,
        [FromServices] IReservationService reservationService)
    {
        var user = await loggedInUserAccessor.Get();

        await reservationService.Move(lanId, seatId, request.ToSeatId, user);

        return TypedResults.Ok();
    }

    public record Request(Guid ToSeatId);

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.ToSeatId).NotEmpty();
        }
    }
}