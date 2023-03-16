using Seatpicker.Application.Features.Reservation.Ports;
using Seatpicker.Domain;

namespace Seatpicker.Application.Features.Reservation;

public interface IReservationService
{
    public Task<Seat> Reserve(User user, Guid seatId);
    public Task UnReserve(User user, Guid seatId);
    public Task<Seat> ReplaceReservation(User user, Guid oldSeatId, Guid newSeatId);
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

        if (seat.User is null)
            seat.User = user;
        else
            throw new SeatAlreadyReservedException(seatId);

        await seatRepository.Store(seat);

        return seat;
    }

    public async Task UnReserve(User user, Guid seatId)
    {
        var seat = await seatRepository.Get(seatId) ?? throw new SeatNotFoundException(seatId);

        if (seat.User is null || seat.User.Id != user.Id) throw new SeatAlreadyReservedException(seatId);
        seat.User = null;

        await seatRepository.Store(seat);
    }

    public Task<Seat> ReplaceReservation(User user, Guid oldSeatId, Guid newSeatId)
    {
        var oldSeatTask = seatRepository.Get(oldSeatId);
        var newSeatTask = seatRepository.Get(newSeatId);

        Task.WaitAll(oldSeatTask, newSeatTask);

        var oldSeat = oldSeatTask.Result ?? throw new SeatNotFoundException(oldSeatId);
        var newSeat = newSeatTask.Result ?? throw new SeatNotFoundException(newSeatId);

        if (newSeat.User is not null) throw new SeatAlreadyReservedException(newSeatId);
        if (oldSeat.User is not null && oldSeat.User.Id != user.Id) throw new SeatAlreadyReservedException(oldSeatId);
        newSeat.User = user;
        oldSeat.User = null;

        Task.WaitAll(seatRepository.Store(oldSeat), seatRepository.Store(newSeat));

        return Task.FromResult(newSeat);
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

