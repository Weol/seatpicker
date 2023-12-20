using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Seats;
using Seatpicker.Infrastructure.Entrypoints.Utils;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Reservation;

[ApiController]
[Route("lan/{lanId:Guid}/seat/{seatId:guid}/reservation")]
[Authorize]
public class MoveEndpoint
{
    [HttpPut]
    public async Task<IActionResult> Move(
        [FromRoute] Guid lanId,
        [FromRoute] Guid seatId,
        [FromBody] Request request,
        [FromServices] ILoggedInUserAccessor loggedInUserAccessor,
        [FromServices] IReservationService reservationService)
    {
        var user = await loggedInUserAccessor.Get();

        await reservationService.Move(lanId, seatId, request.ToSeatId, user);

        return new OkResult();
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