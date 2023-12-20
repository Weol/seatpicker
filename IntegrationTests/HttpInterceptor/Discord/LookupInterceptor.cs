using System.Net;
using System.Net.Http.Headers;
using Seatpicker.Infrastructure.Authentication.Discord.DiscordClient;

namespace Seatpicker.IntegrationTests.HttpInterceptor.Discord;

public class LookupInterceptor : IInterceptor
{
    private readonly DiscordUser discordUser;

    public LookupInterceptor(DiscordUser discordUser)
    {
        this.discordUser = discordUser;
    }

    public bool Match(string uri, HttpHeaders headers, HttpRequestMessage request)
    {
        return uri.EndsWith("users/@me");
    }

    public (object Response, HttpStatusCode Code) Response(HttpRequestMessage requestMessage)
    {
        var response = new
        {
            id = discordUser.Id,
            username = discordUser.Username,
            discriminator = "1337",
            avatar = discordUser.Avatar ?? null
        };

        return (response, HttpStatusCode.OK);
    }
}