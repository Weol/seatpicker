using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Lans;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Lan;

public static class UpdateLan
{
    public static async Task<IResult> Update(
        [FromRoute] string guildId,
        [FromRoute] Guid lanId,
        [FromBody] Request request,
        ILoggedInUserAccessor loggedInUserAccessor,
        ILanManagementService lanManagementService)
    {
        if (lanId != request.Id) throw new BadRequestException("Route parameter id does not match the request model id");

        var user = await loggedInUserAccessor.GetUser();

        await lanManagementService.Update(request.Id, request.Active, request.Title, request.Background, user);

        return TypedResults.Ok();
    }

    public record Request(Guid Id, bool Active, string Title, byte[] Background);

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