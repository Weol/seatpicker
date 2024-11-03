using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Lan;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Lan;

public static class UpdateLan
{
    public static async Task<IResult> Update(
        [FromRoute] string guildId,
        [FromRoute] string lanId,
        [FromBody] Request request,
        [FromServices] ILoggedInUserAccessor loggedInUserAccessor,
        [FromServices] LanService lanService)
    {
        if (lanId != request.Id)
            throw new BadRequestException("Route parameter id does not match the request model id");

        var user = await loggedInUserAccessor.GetUser();

        await lanService.UpdateBackground(request.Id, request.Background, user);
        await lanService.UpdateTitle(request.Id, request.Title, user);
        await lanService.SetActive(request.Id, request.Active, user);

        return TypedResults.Ok();
    }

    public record Request(string Id, bool Active, string Title, byte[] Background);

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();

            RuleFor(x => x.Title).NotEmpty();

            RuleFor(x => x.Background)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(SvgUtils.IsSvg)
                .WithMessage("Background must be a valid svg");
        }
    }
}