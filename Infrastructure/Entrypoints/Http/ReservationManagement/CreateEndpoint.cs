using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Seats;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Entrypoints.Utils;

namespace Seatpicker.Infrastructure.Entrypoints.Http.ReservationManagement;

[ApiController]
[Route("guild/{guildId}/lan/{lanId:Guid}/seat/{seatId:Guid}/reservationmanagement")]
[Authorize(Roles = "Operator")]
public class CreateEndpoint
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromRoute] Guid lanId,
        [FromRoute] Guid seatId,
        [FromBody] Request request,
        [FromServices] ILoggedInUserAccessor loggedInUserAccessor,
        [FromServices] IReservationManagementService reservationManagementService)
    {
        var user = await loggedInUserAccessor.Get();

        await reservationManagementService.Create(lanId, seatId, new UserId(request.UserId), user);

        return new OkResult();
    }

    public record Request(string UserId);

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty();
        }
    }
}