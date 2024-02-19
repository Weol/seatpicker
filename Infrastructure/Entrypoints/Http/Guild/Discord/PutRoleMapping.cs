using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Adapters.Database.GuildRoleMapping;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Guild.Discord;

public static class PutRoleMapping 
{
    public static async Task<IResult> Put(
        [FromServices] GuildRoleMappingRepository guildRoleRepository,
        [FromBody] IEnumerable<Request> request,
        [FromRoute] string guildId)
    {
        await guildRoleRepository.SaveRoleMapping(
            guildId,
            request.SelectMany(guildRole => guildRole.Roles.Select(role => (guildRole.Id, role))));
        
        return TypedResults.Ok();
    }

    public record Request(string Id, IEnumerable<Role> Roles);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.Id).NotEmpty();

            RuleFor(x => x.Roles).NotNull();
        }
    }
}