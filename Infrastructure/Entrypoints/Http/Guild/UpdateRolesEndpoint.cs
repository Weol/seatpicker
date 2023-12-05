using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Infrastructure.Authentication;
using Seatpicker.Infrastructure.Authentication.Discord;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Guild;

[ApiController]
[Route("guild/{guildId}")]
[Area("guilds")]
[Authorize(Roles = "Admin")]
public class UpdateRolesEndpoint
{
    [HttpPut("roles")]
    public async Task<IActionResult> Set(
        [FromServices] DiscordAuthenticationService discordAuthenticationService,
        [FromBody] IEnumerable<Request> request,
        [FromRoute] string guildId)
    {
        await discordAuthenticationService.SetRoleMapping(
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