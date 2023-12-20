using System.Net;
using System.Net.Http.Headers;

namespace Seatpicker.IntegrationTests.HttpInterceptor.Discord;

public class RefreshTokenInterceptor : IInterceptor
{
    public bool Match(string uri, HttpHeaders headers, HttpRequestMessage request)
    {
        if (request.Content is null) return false;
        
        var stringContent = request.Content!.ReadAsStringAsync()
            .GetAwaiter()
            .GetResult();
        
        return stringContent.Contains("refresh_token") && uri.EndsWith("oauth2/token");
    }

    public (object Response, HttpStatusCode Code) Response(HttpRequestMessage requestMessage)
    {
        var response = new
        {
            access_token = "9283hfaojflsjjdskjfa3ijdsfasfd",
            token_type = "Bearer",
            expires_in = 604800,
            refresh_token = "dfafjajfoijadsløfjaaøeoi32",
            scope = "identify"
        };

        return (response, HttpStatusCode.OK);
    }
}