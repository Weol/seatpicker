using System.ComponentModel.Design;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using NSubstitute;

namespace IntegrationTests.Host;

public class HttpRequestFaker
{
    private IHttpRequestHandler requestHandler;

    public HttpRequestFaker(IHttpRequestHandler requestHandler)
    {
        this.requestHandler = requestHandler;
    }

    public void WhenUri(Uri expectedUri, Func<HttpRequestMessage, HttpResponseMessage> returnResponse)
    {
        requestHandler
            .Handle(Arg.Is<HttpRequestMessage>(request => expectedUri == request.RequestUri))
            .Returns(info =>
            {
                var request = info.Arg<HttpRequestMessage>();
                return returnResponse(request);
            });
    }
}

public static class HttpRequestFakerHostExtensions
{
    public static IServiceCollection AddHttpRequestFaker(this IServiceCollection services)
    {
        return services.AddSingleton<HttpRequestFaker>()
            .AddSingleton(_ => Substitute.For<IHttpRequestHandler>())
            .AddScoped<RequestHandler>()
            .ConfigureAll<HttpClientFactoryOptions>(options =>
        {
            options.HttpMessageHandlerBuilderActions.Add(builder =>
            {
                builder.AdditionalHandlers.Add(builder.Services.GetRequiredService<RequestHandler>());
            });
        });
    }

    private class RequestHandler : DelegatingHandler
    {
        private readonly IHttpRequestHandler requestHandler;

        public RequestHandler(IHttpRequestHandler requestHandler)
        {
            this.requestHandler = requestHandler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return requestHandler.Handle(request);
        }
    }
}

public interface IHttpRequestHandler
{
    Task<HttpResponseMessage> Handle(HttpRequestMessage requestMessage);
}