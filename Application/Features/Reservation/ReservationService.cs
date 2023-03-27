using Seatpicker.Domain;
using Shared;

namespace Seatpicker.Application.Features.Reservation;

public interface IReservationService
{
    public Task<Seat> Reserve(User user, Guid seatId);
    public Task UnReserve(User user, Guid seatId);
    public Task<Seat> SwitchReservation(User user, Guid oldSeatId, Guid newSeatId);
}

internal class ReservationService : IReservationService
{
    private readonly ISeatRepository seatRepository;
    private readonly IDomainEventPublisher domainEventPublisher;

    public ReservationService(ISeatRepository seatRepository, IDomainEventPublisher domainEventPublisher)
    {
        this.seatRepository = seatRepository;
        this.domainEventPublisher = domainEventPublisher;
    }

    public async Task<Seat> Reserve(User user, Guid seatId)
    {
        var seat = await seatRepository.Get(seatId) ?? throw new SeatNotFoundException(seatId);
        var reservedSeat = await seatRepository.GetByUser(user.Id);

        if (reservedSeat is not null) throw new DuplicateSeatReservationException(user);

        if (seat.User is null)
            seat.User = user;
        else
            throw new SeatUnavailableException(seatId);

        await seatRepository.Store(seat);

        await domainEventPublisher.Publish(new SeatReservedEvent(seat.Id, user));

        return seat;
    }

    public async Task UnReserve(User user, Guid seatId)
    {
        var seat = await seatRepository.Get(seatId) ?? throw new SeatNotFoundException(seatId);

        if (seat.User is null || seat.User.Id != user.Id) throw new SeatUnavailableException(seatId);
        seat.User = null;

        await seatRepository.Store(seat);
    }

    public Task<Seat> SwitchReservation(User user, Guid oldSeatId, Guid newSeatId)
    {
        var oldSeatTask = seatRepository.Get(oldSeatId);
        var newSeatTask = seatRepository.Get(newSeatId);

        Task.WaitAll(oldSeatTask, newSeatTask);

        var oldSeat = oldSeatTask.Result ?? throw new SeatNotFoundException(oldSeatId);
        var newSeat = newSeatTask.Result ?? throw new SeatNotFoundException(newSeatId);

        if (newSeat.User is not null) throw new SeatUnavailableException(newSeatId);
        if (oldSeat.User is not null && oldSeat.User.Id != user.Id) throw new SeatUnavailableException(oldSeatId);
        newSeat.User = user;
        oldSeat.User = null;

        Task.WaitAll(seatRepository.Store(oldSeat), seatRepository.Store(newSeat));

        return Task.FromResult(newSeat);
    }
}

public class SeatNotFoundException : DomainException {

    public Guid SeatId { get; }

    public SeatNotFoundException(Guid seatId) : base($"Seat with ID ${seatId} not found")
    {
        SeatId = seatId;
    }
}

public class SeatUnavailableException : DomainException {

    public Guid SeatId { get; }

    public SeatUnavailableException(Guid seatId) : base($"Seat with ID ${seatId} is unavailable")
    {
        SeatId = seatId;
    }
}

public class DuplicateSeatReservationException : DomainException {

    public User User { get; }

    public DuplicateSeatReservationException(User user) : base($"User {user.Nick} has already reserved a seat")
    {
        User = user;
    }
}

