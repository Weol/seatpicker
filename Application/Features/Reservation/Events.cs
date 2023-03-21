using Seatpicker.Domain;
using Shared;

namespace Seatpicker.Application.Features.Reservation;

public record SeatReservedEvent(Guid SeatId, User User) : IDomainEvent;