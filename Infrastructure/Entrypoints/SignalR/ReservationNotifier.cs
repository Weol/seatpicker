using Microsoft.AspNetCore.SignalR;
using Seatpicker.Application.Features.Reservation;
using Seatpicker.Domain;

namespace Seatpicker.Infrastructure.Entrypoints.SignalR;

public class ReservationNotifier : IReservationNotifier
{
    private readonly IHubContext<ReservationHub> reservationHub;
    private readonly IUserProvider userProvider;

    public ReservationNotifier(IHubContext<ReservationHub> reservationHub, IUserProvider userProvider)
    {
        this.reservationHub = reservationHub;
        this.userProvider = userProvider;
    }

    public async Task NotifySeatReservationChanged(Seat seat)
    {
        var user = seat.ReservedBy is null ? null : await userProvider.GetById(seat.ReservedBy);
        await reservationHub.Clients.All.SendAsync("ReservationChanged", new Response(seat.Id, user));
    }
    
    public record Response(string Id, User? ReservedBy);
}