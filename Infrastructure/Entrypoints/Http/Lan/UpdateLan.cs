using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Lans;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Lan;

public static class UpdateLan
{
    public static async Task<IResult> Update(
        [FromRoute] Guid lanId,
        [FromBody] Request request,
        ILoggedInUserAccessor loggedInUserAccessor,
        ILanManagementService lanManagementService)
    {
        if (lanId != request.Id) throw new BadRequestException("Route parameter id does not match the request model id");

        if (request.Active is null && request.Title is null && request.Background is null)
            throw new BadRequestException("At least one property besides id must be set");

        var user = await loggedInUserAccessor.Get();

        await lanManagementService.Update(request.Id, request.Active, request.Title, request.Background, user);

        return TypedResults.Ok();
    }

    public record Request(Guid Id, bool? Active, string? Title, byte[]? Background);

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();

            RuleFor(x => x.Title).NotEmpty().When(model => model.Title is not null);

            RuleFor(x => x.Background)
                .NotEmpty()
                .DependentRules(
                    () =>
                    {
                        RuleFor(x => x.Background)
                            .Must((_, background) => background is not null && SvgUtils.IsSvg(background))
                            .WithMessage("Background must be a valid svg");
                    })
                .When(model => model.Background is not null);
        }
    }
}