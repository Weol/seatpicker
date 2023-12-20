using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Infrastructure.Adapters.Database.GuildRoleMapping;
using Seatpicker.Infrastructure.Authentication;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Guild;

[ApiController]
[Route("guild/{guildId}")]
[Authorize(Roles = "Admin")]
public class UpdateRolesEndpoint
{
    [HttpPut("roles")]
    public async Task<IActionResult> Set(
        [FromServices] GuildRoleMappingRepository guildRoleRepository,
        [FromBody] IEnumerable<Request> request,
        [FromRoute] string guildId)
    {
        await guildRoleRepository.SaveRoleMapping(
            guildId,
            request.SelectMany(guildRole => guildRole.Roles.Select(role => (guildRole.Id, role))));
        
        return new OkResult();
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