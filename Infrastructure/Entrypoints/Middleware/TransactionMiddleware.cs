using Seatpicker.Application.Features;
using Seatpicker.Infrastructure.Adapters;

namespace Seatpicker.Infrastructure.Entrypoints.Middleware;

public class TransactionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var guildIdRouteValue = context.GetRouteValue("guildId");

        if (guildIdRouteValue is string guildId)
        {
            var guildIdProvider = context.RequestServices.GetRequiredService<GuildIdProvider>();
            guildIdProvider.GuildId = guildId;

            await using var documentTransaction = context.RequestServices.GetRequiredService<IDocumentTransaction>();
            await using var aggregateTransaction = context.RequestServices.GetRequiredService<IAggregateTransaction>();
            await using var guildlessDocumentTransaction = context.RequestServices.GetRequiredService<IGuildlessDocumentTransaction>();

            await next(context);

            await Task.WhenAll(
                documentTransaction.Commit(),
                aggregateTransaction.Commit(),
                guildlessDocumentTransaction.Commit());
        }
        else
        {
            await using var guildlessDocumentTransaction = context.RequestServices.GetRequiredService<IGuildlessDocumentTransaction>();

            await next(context);

            await guildlessDocumentTransaction.Commit();
        }
    }
}

public static class TransactionMiddlewareExtensions
{
    public static IApplicationBuilder UseTransactionMiddleware(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TransactionMiddleware>();
    }
}