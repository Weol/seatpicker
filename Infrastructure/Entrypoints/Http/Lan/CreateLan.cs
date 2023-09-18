using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.LanManagement;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Lan;

public partial class LanController
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLanRequestModel model)
    {
        await validateModel.Validate<CreateLanRequestModel, CreateLanRequestModelValidator>(model);

        var user = loggedInUserAccessor.Get();

        var lanId = await lanManagementService.Create(model.Title, model.Background, user);

        return new CreatedResult(lanId.ToString(), null);
    }

    public record CreateLanRequestModel(string Title, byte[] Background);

    private class CreateLanRequestModelValidator : AbstractValidator<CreateLanRequestModel>
    {
        public CreateLanRequestModelValidator()
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