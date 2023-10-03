using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Seats;
using Seatpicker.Infrastructure.Entrypoints.Http.Utils;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Seat;

[ApiController]
[Route("seat")]
public class Update
{
    [HttpPut("{id:guid}")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Endpoint(
        [FromRoute] Guid id,
        [FromBody] Request request,
        [FromServices] ILoggedInUserAccessor loggedInUserAccessor,
        [FromServices] ISeatManagementService seatManagementService)
    {
        if (id != request.SeatId)
            throw new BadRequestException(
                $"Route parameter {nameof(id)} does not match the request model {nameof(Request.SeatId)}");

        var user = await loggedInUserAccessor.Get();

        await seatManagementService.Update(request.SeatId, request.Title, request.Bounds?.ToDomainBounds(), user);

        return new OkResult();
    }

    public record Request(Guid SeatId, string? Title, Bounds? Bounds);

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.SeatId).NotEmpty();

            RuleFor(x => x.Title)
                .NotEmpty()
                .When(x => x.Title is not null);

            RuleFor(x => x.Bounds)
                .SetValidator(new BoundsValidator()!)
                .When(x => x.Bounds is not null);
        }
    }
}