namespace Seatpicker.Infrastructure.Entrypoints.Http.Seat;

public record SeatResponse(Guid Id, string Title, Bounds Bounds, User? ReservedBy);