using Microsoft.Extensions.Logging;
using Seatpicker.Application.Features.Reservation.Ports;
using Seatpicker.Domain;

namespace Seatpicker.Application.Features.Reservation;

public interface IReservationService
{
    public Task<Seat> Reserve(User user, Guid seatId);
}

internal class ReservationService : IReservationService
{
    private readonly ILogger<ReservationService> logger;
    private readonly ISeatRepository seatRepository;

    public async Task<Seat> Reserve(User user, Guid seatId)
    {
        var seat = await seatRepository.Get(seatId) ?? new Seat
        {
            Id = seatId,
        };

        seat.User = user;

        await seatRepository.Store(seat);

        return seat;
    }
}

