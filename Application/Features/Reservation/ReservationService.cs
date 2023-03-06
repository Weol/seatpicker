using Seatpicker.Application.Features.Reservation.Ports;
using Seatpicker.Domain;

namespace Seatpicker.Application.Features.Reservation;

public interface IReservationService
{
    public Task<Seat> Reserve(User user, Guid seatId);
    public Task UnReserve(Guid seatId);
}

internal class ReservationService : IReservationService
{
    private readonly ISeatRepository seatRepository;

    public ReservationService(ISeatRepository seatRepository)
    {
        this.seatRepository = seatRepository;
    }

    public async Task<Seat> Reserve(User user, Guid seatId)
    {
        var seat = await seatRepository.Get(seatId) ?? throw new SeatNotFoundException(seatId);

        if (seat.User is not null)
            seat.User = user;
        else
            throw new SeatAlreadyReservedException(seatId);

        await seatRepository.Store(seat);

        return seat;
    }

    public async Task UnReserve(Guid seatId)
    {
        var seat = await seatRepository.Get(seatId) ?? throw new SeatNotFoundException(seatId);

        seat.User = null;

        await seatRepository.Store(seat);
    }
}

public class SeatNotFoundException : Exception {

    public Guid SeatId { get; }

    public SeatNotFoundException(Guid seatId) : base($"Seat with ID ${seatId} not found")
    {
        SeatId = seatId;
    }
}

public class SeatAlreadyReservedException : Exception {

    public Guid SeatId { get; }

    public SeatAlreadyReservedException(Guid seatId) : base($"Seat with ID ${seatId} is already reserved")
    {
        SeatId = seatId;
    }
}

