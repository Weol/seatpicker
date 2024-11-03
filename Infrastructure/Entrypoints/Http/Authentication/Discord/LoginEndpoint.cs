using Microsoft.AspNetCore.Mvc;
using Seatpicker.Infrastructure.Authentication.Discord;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Authentication.Discord;

public static class LoginEndpoint
{
    public static async Task<IResult> Login(
        [FromServices] DiscordAuthenticationService discordAuthenticationService,
        [FromBody] Request request)
    {
        var (jwtToken, expiresAt, discordToken)
            = await discordAuthenticationService.Login(request.Token, request.GuildId, request.RedirectUrl);

        return TypedResults.Ok(new TokenResponse(jwtToken,
            request.GuildId,
            expiresAt.ToUnixTimeSeconds(),
            discordToken.RefreshToken,
            discordToken.Id,
            discordToken.Nick,
            discordToken.Avatar,
            discordToken.Roles));
    }

    public record Request(string Token, string? GuildId, string RedirectUrl);
}