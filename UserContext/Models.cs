namespace Seatpicker.Domain;

public record User(string Id, string Nick, string Name, IEnumerable<string> Claims, DateTime CreatedAt); 

public record UnregisteredUser(string Id, string Nick, string Name); 
