namespace Seatpicker.IntegrationTests;

public class InterceptingHttpMessageHandler : HttpMessageHandler
{
    public ICollection<Interceptor> Interceptors = new List<Interceptor>();

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Handle(request));
    }

    public HttpResponseMessage Handle(HttpRequestMessage request)
    {
        try
        {
            var interceptor = Interceptors.First(interceptor => interceptor.Matcher(request));
            return interceptor.Generator(request);
        }
        catch (InvalidOperationException)
        {
            throw new Exception($"No interceptor found that matches request: {new { request.RequestUri, request.Method }}");
        }
    }

    public record Interceptor(Predicate<HttpRequestMessage> Matcher, Func<HttpRequestMessage, HttpResponseMessage> Generator);
}