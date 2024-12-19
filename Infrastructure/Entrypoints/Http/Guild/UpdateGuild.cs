using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Lan;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Guild;

public static class UpdateGuild
{
    public static async Task<IResult> Update(
        [FromRoute] string guildId,
        [FromBody] Request request,
        [FromServices] ILoggedInUserAccessor loggedInUserAccessor,
        [FromServices] GuildService guildService)
    {
        if (guildId != request.Id)
            throw new BadRequestException("Route parameter id does not match the request model id");

        var user = await loggedInUserAccessor.GetUser();

        var guild = await guildService.Update(request.ToGuild(), user);

        return TypedResults.Ok(guild);
    }

    public record Request(string Id, string Name, string? Icon, string[] Hostnames, GuildRoleMapping[] RoleMapping, GuildRole[] Roles)
    {
        public Application.Features.Lan.Guild ToGuild() =>
            new (Id, Name, Icon, Hostnames, RoleMapping, Roles); 
    };

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty();

            RuleFor(x => x.Name).NotNull().NotEmpty();

            RuleFor(x => x.Icon).NotEmpty().When(x => x.Icon != null);

            RuleFor(x => x.Hostnames)
                .NotNull()
                .Must(x => x.Distinct().Count() == x.Length)
                .WithMessage("No duplicate hostnames are allowed");
        }

    }
}