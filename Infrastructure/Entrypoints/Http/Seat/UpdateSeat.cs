using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Reservation;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Seat;

public static class UpdateSeat
{
    public static async Task<IResult> Update(
        [FromRoute] string guildId,
        [FromRoute] string lanId,
        [FromRoute] string seatId,
        [FromBody] Request request,
        [FromServices] ILoggedInUserAccessor loggedInUserAccessor,
        [FromServices] SeatManagementService seatManagementService)
    {
        var user = await loggedInUserAccessor.GetUser();

        await seatManagementService.UpdateBounds(seatId, request.Bounds.ToDomainBounds(), user);

        return TypedResults.Ok();
    }

    public record Request(string Title, Bounds Bounds);

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Title).NotEmpty();

            RuleFor(x => x.Bounds).SetValidator(new BoundsValidator()!);
        }
    }
}