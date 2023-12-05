using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Lans;
using Seatpicker.Infrastructure.Entrypoints.Utils;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Lan;

[ApiController]
[Route("lan")]
[Area("lan")]
[Authorize(Roles = "Admin")]
public class CreateEndpoint
{
    [HttpPost]
    [ProducesResponseType(200)]
    public async Task<ActionResult<Response>> Create(
        [FromBody] Request request,
        ILoggedInUserAccessor loggedInUserAccessor,
        ILanManagementService lanManagementService)
    {
        var user = await loggedInUserAccessor.Get();

        var lanId = await lanManagementService.Create(request.Title, request.GuildId, request.Background, user);

        return new OkObjectResult(new Response(lanId));
    }

    public record Request(string GuildId, string Title, byte[] Background);

    public record Response(Guid Id);

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Title)
                .NotEmpty();
            
            RuleFor(x => x.GuildId)
                .NotEmpty();

            RuleFor(x => x.Background)
                .NotEmpty()
                .Must((_, background) => SvgUtils.IsSvg(background))
                .WithMessage("Background must be a valid svg");
        }
    }
}