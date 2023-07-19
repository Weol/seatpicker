﻿using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Lan;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Management.Lan;

public partial class LanController
{
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateLanRequestModel model)
    {
        await validateModel.Validate<UpdateLanRequestModel, UpdateLanRequestModelValidator>(model);

        if (id != model.Id) throw new ValidationException("Route parameter id does not match the request model id");

        await lanService.Update(new UpdateLan(model.Id, model.Title, model.Background));

        return new OkResult();
    }

    public record UpdateLanRequestModel(Guid Id, string? Title, byte[]? Background);

    private class UpdateLanRequestModelValidator : AbstractValidator<UpdateLanRequestModel>
    {
        public UpdateLanRequestModelValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty();

            RuleFor(x => x.Title)
                .NotEmpty()
                .When(model => model.Title is not null);

            RuleFor(x => x.Background)
                .NotEmpty()
                .DependentRules(() =>
                {
                    RuleFor(x => x.Background)
                        .Must((_, background) => IsSvg(background))
                        .WithMessage("Background must be a valid svg");
                })
                .When(model => model.Background is not null);
        }
    }
}