using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Seats;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Seat;

public static class UpdateSeat
{
    public static async Task<IResult> Update(
        [FromRoute] string guildId,
        [FromRoute] Guid lanId,
        [FromRoute] Guid seatId,
        [FromBody] Request request,
        [FromServices] ILoggedInUserAccessor loggedInUserAccessor,
        [FromServices] ISeatManagementService seatManagementService)
    {
        var user = await loggedInUserAccessor.Get();

        await seatManagementService.Update(lanId, seatId, request.Title, request.Bounds?.ToDomainBounds(), user);

        return TypedResults.Ok();
    }

    public record Request(string? Title, Bounds? Bounds);

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .When(x => x.Title is not null);

            RuleFor(x => x.Bounds)
                .SetValidator(new BoundsValidator()!)
                .When(x => x.Bounds is not null);
        }
    }
}