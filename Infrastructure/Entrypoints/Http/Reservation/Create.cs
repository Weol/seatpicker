using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Seats;
using Seatpicker.Infrastructure.Entrypoints.Http.Utils;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Reservation;

[ApiController]
[Route("reservation")]
public class Create
{
    [HttpPost]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Endpoint(
        [FromBody] Request request,
        [FromServices] ILoggedInUserAccessor loggedInUserAccessor,
        [FromServices] IReservationService reservationService)
    {
        var user = await loggedInUserAccessor.Get();

        await reservationService.Create(request.SeatId, user);

        return new OkResult();
    }

    public record Request(Guid SeatId);

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.SeatId)
                .NotEmpty();
        }
    }
}