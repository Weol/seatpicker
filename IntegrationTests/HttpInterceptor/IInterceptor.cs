using System.Net;
using System.Net.Http.Headers;

namespace Seatpicker.IntegrationTests.HttpInterceptor;

public interface IInterceptor
{
    public bool Match(string uri, HttpHeaders headers, HttpRequestMessage request);
    
    public (object? Response, HttpStatusCode Code) Response(HttpRequestMessage request);
}