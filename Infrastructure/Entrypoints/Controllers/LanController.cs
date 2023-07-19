using System.Text;
using System.Text.RegularExpressions;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Seatpicker.Application.Features.Lan;
using Seatpicker.Infrastructure.Utils;

namespace Seatpicker.Infrastructure.Entrypoints.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(Roles = "Admin")]
public class LanController
{
    private readonly ILanService lanService;
    private readonly IValidateModel validateModel;

    public LanController(ILanService lanService, IValidateModel validateModel)
    {
        this.lanService = lanService;
        this.validateModel = validateModel;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLanRequestModel model)
    {
        await validateModel.Validate<CreateLanRequestModel, CreateLanRequestModelValidator>(model);

        await lanService.Create(new CreateLan(model.Id, model.Title, model.Background));

        return new OkResult();
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateLanRequestModel model)
    {
        await validateModel.Validate<UpdateLanRequestModel, UpdateLanRequestModelValidator>(model);

        if (id != model.Id) throw new ValidationException("Route parameter id does not match the request model id");

        await lanService.Update(new UpdateLan(model.Id, model.Title, model.Background));

        return new OkResult();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var lan = await lanService.Get(id);

        return new OkObjectResult(new LanResponseModel(lan.Id, lan.Title));
    }

    public record CreateLanRequestModel(Guid Id, string Title, byte[] Background);

    public record UpdateLanRequestModel(Guid Id, string? Title, byte[]? Background);

    public record LanResponseModel(Guid Id, string Title);

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
                .Must((model, background) => IsSvg(background))
                    .WithMessage("Background must be a valid svg");
        }
    }

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
                        .Must((model, background) => IsSvg(background))
                        .WithMessage("Background must be a valid svg");
                })
                .When(model => model.Background is not null);
        }
    }

    private static bool IsSvg(byte[]? svgImage)
    {
        if (svgImage is null) return false;

        var utf8 = new UTF8Encoding();
        var svgString = utf8.GetString(svgImage);

        return Regex.IsMatch(svgString, @"^\n*(<[!?].+>\n*){0,2}\n*<svg.+\/svg>\n*$");
    }
}