using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Seats;

namespace Seatpicker.Infrastructure.Entrypoints.Http.ReservationManagement;

public static class CreateReservationFor
{
    public static async Task<IResult> Create(
        [FromRoute] string guildId,
        [FromRoute] Guid lanId,
        [FromRoute] Guid seatId,
        [FromBody] Request request,
        [FromServices] ILoggedInUserAccessor loggedInUserAccessor,
        [FromServices] IReservationManagementService reservationManagementService)
    {
        var user = await loggedInUserAccessor.Get();

        await reservationManagementService.Create(lanId, seatId, request.UserId, user);

        return TypedResults.Ok();
    }

    public record Request(string UserId);

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty();
        }
    }
}