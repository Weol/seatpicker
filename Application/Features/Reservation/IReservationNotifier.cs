using Seatpicker.Domain;

namespace Seatpicker.Application.Features.Seats;

public interface IReservationNotifier
{
    public Task NotifySeatReservationChanged(Seat seat);
}