using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Lan;
using Seatpicker.Domain;

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
            throw new BadRequestException("Route parameter id doeSaveGuilds not match the request model id");

        var user = await loggedInUserAccessor.GetUser();

        await guildService.Update(request.ToGuild(), user);

        return TypedResults.Ok();
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

            RuleForEach(x => x.Hostnames)
                .NotEmpty()
                .Must(IsValidDomain)
                .WithMessage("All hostnames must be valid DNS hostnames");

            RuleFor(x => x.Hostnames)
                .NotNull()
                .Must(x => x.Distinct().Count() == x.Length)
                .WithMessage("No duplicate hostnames are allowed");

            RuleFor(x => x.RoleMapping)
                .NotNull()
                .Must(HaveNoDuplicateRolesAcrossMappings)
                .WithMessage("Role mapping cannot contain duplicates of the same role across mappings");
        }

        private static bool IsValidDomain(string hostname)
        {
            return Uri.CheckHostName(hostname) != UriHostNameType.Unknown;
        }

        private static bool HaveNoDuplicateRolesAcrossMappings(GuildRoleMapping[] roleMapping)
        {
            var allRoles = roleMapping.SelectMany(mapping => mapping.Roles).ToList();
            return allRoles.Distinct().Count() == allRoles.Count;
        }
    }
}