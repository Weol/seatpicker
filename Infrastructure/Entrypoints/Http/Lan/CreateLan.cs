using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Lan;

public partial class LanController
{
    [HttpPost]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Create([FromBody] CreateLanRequest model)
    {
        await validateModel.Validate<CreateLanRequest, CreateLanRequestValidator>(model);

        var user = loggedInUserAccessor.Get();

        var lanId = await lanManagementService.Create(model.Title, model.Background, user);

        return new CreatedResult(lanId.ToString(), null);
    }

    public record CreateLanRequest(string Title, byte[] Background);

    private class CreateLanRequestValidator : AbstractValidator<CreateLanRequest>
    {
        public CreateLanRequestValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty();

            RuleFor(x => x.Background)
                .NotEmpty()
                .Must((_, background) => IsSvg(background))
                .WithMessage("Background must be a valid svg");
        }
    }
}