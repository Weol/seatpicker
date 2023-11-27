using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Infrastructure.Authentication;
using Seatpicker.Infrastructure.Authentication.Discord;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Guild.RoleMapping;

[ApiController]
[Route("api/guild/{guildId}/roles/mapping")]
[Area("guilds")]
[Authorize(Roles = "Admin")]
public class SetEndpoint
{
    [HttpPut("")]
    public async Task<IActionResult> Set(
        [FromServices] DiscordAuthenticationService discordAuthenticationService,
        [FromBody] IEnumerable<Request> request,
        [FromRoute] string guildId)
    {
        await discordAuthenticationService.SetRoleMapping(
            guildId, 
            request.Select(mapping => (mapping.RoleId, mapping.Role)));
        return new OkResult();
    }
    
    public record Request(string RoleId,
        Role Role);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.Role).NotEmpty();
            
            RuleFor(x => x.RoleId).NotEmpty();
        }
    }
}