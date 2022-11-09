namespace Seatpicker.Domain;

public record User(string Id, string EmailId, string Nick, string Name, IEnumerable<string> Claims, DateTimeOffset CreatedAt); 

public record UnregisteredUser(string EmailId, string Nick, string Name); 
