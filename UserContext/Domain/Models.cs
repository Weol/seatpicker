namespace Seatpicker.UserContext.Domain;

public record User(
    string Id, 
    string Nick, 
    string Avatar,
    IEnumerable<Role> Roles, 
    DateTimeOffset CreatedAt);

public record UnregisteredUser(
    string Id,
    string Nick,
    string Avatar,
    string Name);
