using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Lan;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Lan;

public static class CreateLan
{
    public static async Task<IResult> Create(
        [FromBody] Request request,
        [FromRoute] string guildId,
        [FromServices] ILoggedInUserAccessor loggedInUserAccessor,
        [FromServices] LanService lanService)
    {
        var user = await loggedInUserAccessor.GetUser();

        var lanId = await lanService.Create(request.Title, request.Background, user);

        return TypedResults.Ok(new Response(lanId));
    }

    public record Request(string Title, byte[] Background);

    public record Response(string Id);

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Title).NotEmpty();

            RuleFor(x => x.Background)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(SvgUtils.IsSvg)
                .WithMessage("Background must be a valid svg");
        }
    }
}