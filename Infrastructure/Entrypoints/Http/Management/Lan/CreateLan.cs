﻿using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.LanManagement;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Management.Lan;

public partial class LanController
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLanRequestModel model)
    {
        await validateModel.Validate<CreateLanRequestModel, CreateLanRequestModelValidator>(model);

        await lanManagementService.Create(new CreateLan(model.Id, model.Title, model.Background));

        return new CreatedResult(model.Id.ToString(), null);
    }

    public record CreateLanRequestModel(Guid Id, string Title, byte[] Background);

    private class CreateLanRequestModelValidator : AbstractValidator<CreateLanRequestModel>
    {
        public CreateLanRequestModelValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty();

            RuleFor(x => x.Title)
                .NotEmpty();

            RuleFor(x => x.Background)
                .NotEmpty()
                .Must((_, background) => IsSvg(background))
                .WithMessage("Background must be a valid svg");
        }
    }
}