using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Lan;

public partial class LanController
{
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateLanRequest model)
    {
        await validateModel.Validate<UpdateLanRequest, UpdateLanRequestValidator>(model);

        if (id != model.Id) throw new BadRequestException("Route parameter id does not match the request model id");

        if (model.Title is null && model.Background is null)
            throw new BadRequestException("At least one property besides id must be set");

        var user = loggedInUserAccessor.Get();

        await lanManagementService.Update(model.Id, model.Title, model.Background, user);

        return new OkResult();
    }

    public record UpdateLanRequest(Guid Id, string? Title, byte[]? Background);

    private class UpdateLanRequestValidator : AbstractValidator<UpdateLanRequest>
    {
        public UpdateLanRequestValidator()
        {
            RuleFor(x => x.Id).NotEmpty();

            RuleFor(x => x.Title).NotEmpty().When(model => model.Title is not null);

            RuleFor(x => x.Background)
                .NotEmpty()
                .DependentRules(
                    () =>
                    {
                        RuleFor(x => x.Background)
                            .Must((_, background) => IsSvg(background))
                            .WithMessage("Background must be a valid svg");
                    })
                .When(model => model.Background is not null);
        }
    }
}