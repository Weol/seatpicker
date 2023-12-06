using Microsoft.AspNetCore.SignalR;
using Seatpicker.Application.Features.Seats;
using Seatpicker.Domain;
using User = Seatpicker.Infrastructure.Entrypoints.Http.User;

namespace Seatpicker.Infrastructure.Adapters.SignalR;

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
        await reservationHub.Clients.All.SendAsync("ReservationChanged", new Response(seat.Id, user is null ? null : User.FromDomainUser(user)));
    }
    
    public record Response(Guid Id, User? ReservedBy);
}