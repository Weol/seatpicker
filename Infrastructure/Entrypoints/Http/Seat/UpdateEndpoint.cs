using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Seats;
using Seatpicker.Infrastructure.Entrypoints.Utils;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Seat;

[ApiController]
[Route("lan/{lanId:guid}/seat")]
[Area("seat")]
[Authorize(Roles = "Operator")]
public class UpdateEndpoint
{
    [HttpPut("{seatId:guid}")]
    public async Task<IActionResult> Update(
        [FromRoute] Guid lanId,
        [FromRoute] Guid seatId,
        [FromBody] Request request,
        [FromServices] ILoggedInUserAccessor loggedInUserAccessor,
        [FromServices] ISeatManagementService seatManagementService)
    {
        var user = await loggedInUserAccessor.Get();

        await seatManagementService.Update(lanId, seatId, request.Title, request.Bounds?.ToDomainBounds(), user);

        return new OkResult();
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