using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests;

public class HttpResponseLoggerHandler : DelegatingHandler
{
    private readonly ITestOutputHelper testOutputHelper;

    public HttpResponseLoggerHandler(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        
        testOutputHelper.WriteLine($"{request.Method} to \"{request.RequestUri!.PathAndQuery}\" has status {response.StatusCode}");
        testOutputHelper.WriteLine($"Body:");
        testOutputHelper.WriteLine(body);

        return response;
    }
}