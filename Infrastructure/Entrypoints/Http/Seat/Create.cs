using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Seats;
using Seatpicker.Infrastructure.Entrypoints.Http.Utils;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Seat;

[ApiController]
[Route("seat")]
public class Create
{
    [HttpPost]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Endpoint(
        [FromBody] Request request,
        [FromServices] ILoggedInUserAccessor loggedInUserAccessor,
        [FromServices] ISeatManagementService seatManagementService)
    {
        var user = await loggedInUserAccessor.Get();

        var seatId = await seatManagementService.Create(request.Title, request.Bounds.ToDomainBounds(), user);

        return new CreatedResult(seatId.ToString(), null);
    }

    public record Request(string Title, Bounds Bounds);

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Title).NotEmpty();

            RuleFor(x => x.Bounds).SetValidator(new BoundsValidator());
        }
    }
}