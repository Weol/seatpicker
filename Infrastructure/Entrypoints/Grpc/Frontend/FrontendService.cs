using System.Collections.Concurrent;
using Grpc.Core;
using Seatpicker.Domain;

namespace Seatpicker.Infrastructure.Entrypoints.Grpc.Frontend;

internal class FrontendService : Frontened.FrontenedBase
{
    private readonly BlockingCollection<(Guid seatId, User user)> events = new ();

    public FrontendService(FrontendNotifier frontendNotifier)
    {
        frontendNotifier.Add((FrontendNotifier.SeatReserved) OnSeatReserved);
    }

    private Task OnSeatReserved(Guid seatId, User user)
    {
        events.Add((seatId, user));
    }

    public override Task SubscribeToSeatReserved(SubscribeRequest request, IServerStreamWriter<SeatReservedResponse> responseStream, ServerCallContext context)
    {
        while (true)
        {
            var e = events.Take();
            responseStream.WriteAsync(
                new SeatReservedResponse
                {
                    SeatId = e.seatId,
                    User = new User
                    {
                        Avatar = e.user.Avatar,
                        Id = e.user.Id,
                        Nick = e.user.Nick,
                    },
                });
        }
    }
}