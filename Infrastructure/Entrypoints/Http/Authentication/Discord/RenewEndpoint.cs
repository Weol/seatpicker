using Microsoft.AspNetCore.Mvc;
using Seatpicker.Infrastructure.Authentication;
using Seatpicker.Infrastructure.Authentication.Discord;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Authentication.Discord;

[ApiController]
[Route("authentication/discord")]
[Area("discord")]
public class RenewEndpoint
{
    [HttpPost("renew")]
    public async Task<ActionResult<Response>> Renew(
        [FromServices] DiscordAuthenticationService discordAuthenticationService,
        [FromBody] Request request)
    {
        var (token, expiresAt, refreshToken, discordUser, roles) = await discordAuthenticationService.Renew(request.RefreshToken, request.GuildId);

        return new OkObjectResult(new Response(token, request.GuildId, expiresAt, refreshToken, discordUser.Id, discordUser.Username, discordUser.Avatar, roles));
    }

    public record Request(string RefreshToken, string GuildId);

    public record Response(string Token, string GuildId, DateTimeOffset ExpiresAt, string RefreshToken, string UserId, string Nick, string? Avatar, ICollection<Role> Roles);
}