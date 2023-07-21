namespace Seatpicker.IntegrationTests;

public abstract class InterceptingHttpMessageHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(Handle(request));
    }

    public virtual HttpResponseMessage Handle(HttpRequestMessage request)
    {
        throw new HttpRequestNotMockedException();
    }
}

public class HttpRequestNotMockedException : Exception
{
}