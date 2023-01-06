namespace Seatpicker.SeatContext.Domain.Seat;

public record Seat(Guid Id, Table Table, User? User);
    
public record User(string Id, string Nick, string Avatar);

public record OccupiedSeat(string UserId, );

public record Table(Table? Reference, int X, int Y, int? Width, int? Height);