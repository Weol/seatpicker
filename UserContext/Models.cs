namespace Seatpicker.Domain;

public record User(string Email, string Nick, string Name, IEnumerable<Claim> Claims, DateTimeOffset CreatedAt); 

public record UnregisteredUser(string Email, string Nick, string Name); 
