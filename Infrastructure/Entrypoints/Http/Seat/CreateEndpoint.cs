using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Seats;
using Seatpicker.Infrastructure.Entrypoints.Utils;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Seat;

[ApiController]
[Route("lan/{lanId:Guid}/seat")]
[Authorize(Roles = "Operator")]
public class CreateEndpoint
{
    [HttpPost]
    public async Task<ActionResult<Response>> Create(
        [FromRoute] Guid lanId,
        [FromBody] Request request,
        [FromServices] ILoggedInUserAccessor loggedInUserAccessor,
        [FromServices] ISeatManagementService seatManagementService)
    {
        var user = await loggedInUserAccessor.Get();

        var id = await seatManagementService.Create(lanId, request.Title, request.Bounds.ToDomainBounds(), user);

        return new OkObjectResult(new Response(id));
    }

    public record Request(string Title, Bounds Bounds);

    public record Response(Guid Id);

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Title).NotEmpty();

            RuleFor(x => x.Title).NotEmpty();

            RuleFor(x => x.Bounds).SetValidator(new BoundsValidator());
        }
    }
}