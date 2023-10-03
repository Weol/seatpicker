using Seatpicker.Application.Features;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Utils;

public class RequestScopedAggregateTransactionMiddleware
{
    private readonly RequestDelegate next;

    public RequestScopedAggregateTransactionMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext context, IServiceProvider provider)
    {
        await using var aggregateTransaction = provider.GetRequiredService<IAggregateTransaction>();

        await next(context);
        aggregateTransaction.Commit();
    }
}