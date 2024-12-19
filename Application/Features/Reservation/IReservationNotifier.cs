using Seatpicker.Domain;

namespace Seatpicker.Application.Features.Reservation;

public interface IReservationNotifier
{
    public Task NotifySeatReservationChanged(Seat seat);
}