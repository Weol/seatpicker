namespace Seatpicker.Infrastructure.Entrypoints.Http.Seat;

public record SeatResponse(string Id, string Title, Bounds Bounds, User? ReservedBy);