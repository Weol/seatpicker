using Microsoft.AspNetCore.Mvc;
using Seatpicker.Infrastructure.Adapters.Database;
using Seatpicker.Infrastructure.Authentication.Discord;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Authentication.Discord;

public static class RenewEndpoint
{
    public static async Task<IResult> Renew(
        [FromServices] DiscordAuthenticationService discordAuthenticationService,
        [FromBody] Request request)
    {
        var (jwtToken, expiresAt, discordToken)
            = await discordAuthenticationService.Renew(request.RefreshToken, request.GuildId);

        return TypedResults.Ok(new TokenResponse(jwtToken,
            request.GuildId,
            expiresAt.ToUnixTimeSeconds(),
            discordToken.RefreshToken,
            discordToken.Id,
            discordToken.Nick,
            discordToken.Avatar,
            discordToken.Roles));
    }

    public record Request(string RefreshToken, string GuildId);
}