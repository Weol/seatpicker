using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Adapters.Guilds;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Guild;

public static class UpdateGuild
{
    public static async Task<IResult> Update(
        [FromServices] GuildAdapter guildAdapter,
        [FromRoute] string guildId,
        [FromBody] Request request)
    {
        if (guildId != request.Id)
            throw new BadRequestException("Route parameter id does not match the request model id");

        await guildAdapter.SaveGuild(request.ToGuild());

        return TypedResults.Ok();
    }

    public record RoleMapping(string GuildRoleId, Role[] Roles);

    public record Request(
        string Id,
        string Name,
        string? Icon,
        string[] Hostnames,
        RoleMapping[] RoleMapping,
        GuildRole[] Roles)
    {
        public Adapters.Guilds.Guild ToGuild() =>
            new(Id,
                Name,
                Icon,
                Hostnames,
                RoleMapping.Select(mapping => (mapping.GuildRoleId, mapping.Roles)).ToArray(),
                Roles);
    };

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.Id)
                .NotNull()
                .NotEmpty();

            RuleFor(x => x.Name)
                .NotNull()
                .NotEmpty();

            RuleFor(x => x.Icon)
                .NotEmpty()
                .When(x => x.Icon != null);

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

        private static bool HaveNoDuplicateRolesAcrossMappings(RoleMapping[] roleMapping)
        {
            var allRoles = roleMapping.SelectMany(mapping => mapping.Roles).ToList();
            return allRoles.Distinct().Count() == allRoles.Count;
        }
    }
}