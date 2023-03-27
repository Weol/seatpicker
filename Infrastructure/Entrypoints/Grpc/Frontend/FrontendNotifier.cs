using System.Collections.Concurrent;
using Seatpicker.Application.Features.Reservation;
using Seatpicker.Domain;

namespace Seatpicker.Infrastructure.Entrypoints.Grpc.Frontend;

internal class FrontendNotifier : IFrontendNotifier
{
    internal delegate Task SeatReserved(Guid seatId, User user);

    internal delegate Task SeatUnreserved(Guid seatId, User user);

    private readonly ILogger<FrontendNotifier> logger;
    private readonly ConcurrentBag<WeakReference<SeatReserved>> seatReservedHandlers = new ();
    private readonly ConcurrentBag<WeakReference<SeatUnreserved>> seatUnreservedHandlers = new ();

    public FrontendNotifier(ILogger<FrontendNotifier> logger)
    {
        this.logger = logger;
    }

    public void Add(SeatReserved seatReserved) => seatReservedHandlers.Add(new WeakReference<SeatReserved>(seatReserved));
    public void Add(SeatUnreserved seatUnreserved) => seatUnreservedHandlers.Add(new WeakReference<SeatUnreserved>(seatUnreserved));

    public async void NotifySeatReserved(Guid seatId, User user)
    {
        var tasks = new List<Task>();
        foreach (var handlerReference in seatReservedHandlers)
        {
            handlerReference.TryGetTarget(out var handler);
            if (handler is null)
            {
                seatReservedHandlers.TryTake(out _);
                logger.LogDebug("Handler has been garbage collected, removing reference");
                break;
            }

            try
            {
                tasks.Add(handler(seatId, user));
            }
            catch (Exception e)
            {
                seatReservedHandlers.TryTake(out _);
                logger.LogError(e, "Handler threw an exception, removing handler");
            }
        }

        await Task.WhenAll(tasks.ToArray());
    }
}