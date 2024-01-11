using Newtonsoft.Json;

namespace Seatpicker.IntegrationTests.HttpInterceptor;

public class InterceptingHttpMessageHandler : HttpMessageHandler
{
    public ICollection<IInterceptor> Interceptors = new List<IInterceptor>();

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(Handle(request));
    }

    public HttpResponseMessage Handle(HttpRequestMessage request)
    {
        try
        {
            var interceptor = Interceptors.First(interceptor =>
                interceptor.Match(request.RequestUri!.ToString(), request.Headers, request));

            var (response, code) = interceptor.Response(request);

            if (response is null) return new HttpResponseMessage(code);
            
            var json = JsonConvert.SerializeObject(response);
            return new HttpResponseMessage(code)
            {
                Content = new StringContent(json),
            };
        }
        catch (InvalidOperationException)
        {
            throw new Exception(
                $"No interceptor found that matches request: {new { request.RequestUri, request.Method }}");
        }
    }
}