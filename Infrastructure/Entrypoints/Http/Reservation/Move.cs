using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Seats;
using Seatpicker.Infrastructure.Entrypoints.Http.Utils;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Reservation;

[ApiController]
[Route("reservation")]
public class Move
{
    [HttpPut("{id:guid}")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Endpoint(
        [FromRoute] Guid id,
        [FromBody] Request request,
        [FromServices] ILoggedInUserAccessor loggedInUserAccessor,
        [FromServices] IReservationService reservationService)
    {
        if (id != request.FromSeatId)
            throw new BadRequestException(
                $"Route parameter {nameof(id)} does not match the request model {nameof(Request.FromSeatId)}");

        var user = await loggedInUserAccessor.Get();

        await reservationService.Move(request.FromSeatId, request.ToSeatId, user);

        return new OkResult();
    }

    public record Request(Guid FromSeatId, Guid ToSeatId);

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.FromSeatId).NotEmpty();

            RuleFor(x => x.ToSeatId).NotEmpty();
        }
    }
}