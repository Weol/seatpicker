using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Reservation;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Seat;

public static class CreateSeat
{
    public static async Task<IResult> Create(
        [FromRoute] string lanId,
        [FromBody] Request request,
        [FromServices] ILoggedInUserAccessor loggedInUserAccessor,
        [FromServices] SeatManagementService seatManagementService)
    {
        var user = await loggedInUserAccessor.GetUser();

        var id = await seatManagementService.Create(
                lanId,
                request.Title,
                request.Bounds.ToDomainBounds(),
                user);

        return TypedResults.Ok(new Response(id));
    }

    public record Request(string Title, Bounds Bounds);

    public record Response(string Id);

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Title).NotEmpty();

            RuleFor(x => x.Bounds).SetValidator(new BoundsValidator());
        }
    }
}