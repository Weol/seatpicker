using System.Net;
using System.Net.Http.Headers;
using Bogus;

namespace Seatpicker.IntegrationTests.HttpInterceptor.Discord;

public class GuildRolesInterceptor : IInterceptor
{
    private readonly IEnumerable<string> roles;

    public string EveryoneId { get; }

    public GuildRolesInterceptor(params string[] roles)
    {
        this.roles = roles;

        do
        {
            EveryoneId = new Faker().Random.Int(1).ToString();
        } while (roles.Any(guildRole => guildRole == EveryoneId));
    }

    public bool Match(string uri, HttpHeaders headers, HttpRequestMessage request)
    {
        return uri.EndsWith("roles");
    }

    public (object Response, HttpStatusCode Code) Response(HttpRequestMessage requestMessage)
    {
        var response = roles
            .Select(guildRole => new
            {
                id = guildRole,
                name = "Role name",
                color = 123
            }).Append(
                new
                {
                    id = EveryoneId,
                    name = "@everyone",
                    color = 123
                }
            ).ToArray();

        return (response, HttpStatusCode.OK);
    }
}