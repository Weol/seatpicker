using Seatpicker.Domain;

namespace Seatpicker.Application.Features.Reservation;

public interface IFrontendNotifier
{
    public Task NotifySeatReserved(Guid seatId, User user);
}