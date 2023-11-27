using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Seats;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Entrypoints.Utils;

namespace Seatpicker.Infrastructure.Entrypoints.Http.ReservationManagement;

[ApiController]
[Route("api/lan/{lanId:Guid}/seat/{seatId:guid}/reservationmanagement")]
[Area("reservationmanagement")]
[Authorize(Roles = "Operator")]
public class MoveEndpoint
{
    [HttpPut("")]
    public async Task<IActionResult> Move(
        [FromRoute] Guid lanId,
        [FromRoute] Guid seatId,
        [FromBody] Request request,
        [FromServices] ILoggedInUserAccessor loggedInUserAccessor,
        [FromServices] IReservationManagementService reservationManagementService)
    {
        var user = await loggedInUserAccessor.Get();

        await reservationManagementService.Move(lanId, new UserId(request.UserId), seatId, request.ToSeatId, user);

        return new OkResult();
    }

    public record Request(string UserId, Guid ToSeatId);

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.UserId).NotEmpty();

            RuleFor(x => x.ToSeatId).NotEmpty();
        }
    }
}