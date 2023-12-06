using Microsoft.AspNetCore.Mvc;
using Seatpicker.Infrastructure.Authentication;
using Seatpicker.Infrastructure.Authentication.Discord;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Authentication.Discord;

[ApiController]
[Route("authentication/discord")]
[Area("discord")]
public class LoginEndpoint
{
    [HttpPost("login")]
    public async Task<ActionResult<Response>> Login(
        [FromServices] DiscordAuthenticationService discordAuthenticationService,
        [FromBody] Request request)
    {
        var (token, expiresAt, refreshToken, discordUser, roles) = await discordAuthenticationService.Login(request.Token, request.GuildId, request.RedirectUrl);

        return new OkObjectResult(new Response(token, request.GuildId, expiresAt.ToUnixTimeSeconds(), refreshToken, discordUser.Id, discordUser.Username, discordUser.Avatar, roles));
    }

    public record Request(string Token, string GuildId, string RedirectUrl);

    public record Response(string Token, string GuildId, long ExpiresAt, string RefreshToken, string UserId, string Nick, string? Avatar, ICollection<Role> Roles);
}