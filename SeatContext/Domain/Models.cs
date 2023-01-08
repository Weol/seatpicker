namespace Seatpicker.SeatContext.Domain;

public record TableLayout(Guid Id, IEnumerable<Table> Tables, byte[] Background);

public record Table(Guid Id, Table? ReferenceTable, double X, double Y, double? Width, double? Height);

public record Seat(Table Table, User? User);

public record User(string Id, string Nick, string Avatar);