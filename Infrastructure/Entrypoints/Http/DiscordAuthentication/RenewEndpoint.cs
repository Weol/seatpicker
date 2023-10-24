using Microsoft.AspNetCore.Mvc;
using Seatpicker.Infrastructure.Authentication;
using Seatpicker.Infrastructure.Authentication.Discord;

namespace Seatpicker.Infrastructure.Entrypoints.Http.DiscordAuthentication;

[ApiController]
[Route("authentication/discord")]
[Area("discordAuthentication")]
public class RenewEndpoint
{
    [HttpPost("renew")]
    public async Task<ActionResult<Response>> Renew(
        [FromServices] DiscordAuthenticationService discordAuthenticationService,
        [FromBody] Request request)
    {
        var (token, expiresAt, refreshToken, discordUser, roles) = await discordAuthenticationService.Renew(request.RefreshToken, request.GuildId);

        return new OkObjectResult(new Response(token, expiresAt, refreshToken, discordUser.Id, discordUser.Username, discordUser.Avatar, roles));
    }

    public record Request(string RefreshToken, string? GuildId);

    public record Response(string Token, DateTimeOffset ExpiresAt, string RefreshToken, string UserId, string Nick, string? Avatar, ICollection<Role> Roles);
}