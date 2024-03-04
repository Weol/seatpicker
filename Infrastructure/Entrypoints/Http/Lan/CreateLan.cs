using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Lans;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Lan;

public static class CreateLan
{
    public static async Task<IResult> Create(
        [FromBody] Request request,
        [FromRoute] string guildId,
        ILoggedInUserAccessor loggedInUserAccessor,
        ILanManagementService lanManagementService)
    {
        var user = await loggedInUserAccessor.Get();

        var lanId = await lanManagementService.Create(guildId, request.Title, request.Background, user);

        return TypedResults.Ok(new Response(lanId));
    }

    public record Request(string Title, byte[] Background);

    public record Response(Guid Id);

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