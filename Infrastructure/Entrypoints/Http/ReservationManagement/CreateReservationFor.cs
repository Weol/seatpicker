using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Reservation;

namespace Seatpicker.Infrastructure.Entrypoints.Http.ReservationManagement;

public static class CreateReservationFor
{
    public static async Task<IResult> Create(
        [FromRoute] string guildId,
        [FromRoute] string lanId,
        [FromRoute] string seatId,
        [FromBody] Request request,
        [FromServices] ILoggedInUserAccessor loggedInUserAccessor,
        [FromServices] ReservationManagementService reservationManagementService)
    {
        var user = await loggedInUserAccessor.GetUser();

        await reservationManagementService.Create(lanId, seatId, request.UserId, user);

        return TypedResults.Ok();
    }

    public record Request(string UserId);

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.UserId).NotEmpty();
        }
    }
}