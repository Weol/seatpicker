using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.LanManagement;
using Seatpicker.Infrastructure.Entrypoints.Http.Utils;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Lan;

[ApiController]
[Route("lan")]
public class Create
{
    [HttpPost]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Endpoint(
        [FromBody] Request request,
        ILoggedInUserAccessor loggedInUserAccessor,
        ILanManagementService lanManagementService)
    {
        var user = await loggedInUserAccessor.Get();

        var lanId = await lanManagementService.Create(request.Title, request.Background, user);

        return new CreatedResult(lanId.ToString(), null);
    }

    public record Request(string Title, byte[] Background);

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Title)
                .NotEmpty();

            RuleFor(x => x.Background)
                .NotEmpty()
                .Must((_, background) => SvgUtils.IsSvg(background))
                .WithMessage("Background must be a valid svg");
        }
    }
}