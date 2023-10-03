using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Seats;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Entrypoints.Http.Utils;

namespace Seatpicker.Infrastructure.Entrypoints.Http.ReservationManagement;

[ApiController]
[Route("reservationmanagement")]
public class Move
{
    [HttpPut("{id:guid}")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Endpoint(
        [FromRoute] Guid id,
        [FromBody] Request request,
        [FromServices] ILoggedInUserAccessor loggedInUserAccessor,
        [FromServices] IReservationManagementService reservationManagementService)
    {
        if (id != request.FromSeatId)
            throw new BadRequestException(
                $"Route parameter {nameof(id)} does not match the request model {nameof(Request.FromSeatId)}");

        var user = await loggedInUserAccessor.Get();

        await reservationManagementService.Move(new UserId(request.UserId), request.FromSeatId, request.ToSeatId, user);

        return new OkResult();
    }

    public record Request(string UserId, Guid FromSeatId, Guid ToSeatId);

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.UserId).NotEmpty();

            RuleFor(x => x.FromSeatId).NotEmpty();

            RuleFor(x => x.ToSeatId).NotEmpty();
        }
    }
}