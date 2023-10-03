using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Seats;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Entrypoints.Http.Utils;

namespace Seatpicker.Infrastructure.Entrypoints.Http.ReservationManagement;

[ApiController]
[Route("reservationmanagement")]
public class Create
{
    [HttpPost]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Endpoint(
        [FromBody] Request request,
        [FromServices] ILoggedInUserAccessor loggedInUserAccessor,
        [FromServices] IReservationManagementService reservationManagementService)
    {
        var user = await loggedInUserAccessor.Get();

        await reservationManagementService.Create(request.SeatId, new UserId(request.UserId), user);

        return new OkResult();
    }

    public record Request(Guid SeatId, string UserId);

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.SeatId)
                .NotEmpty();

            RuleFor(x => x.UserId)
                .NotEmpty();
        }
    }
}