namespace Seatpicker.Domain;

public record User(UserId Id, string Name, ICollection<Role> Roles);

public record UserId(string Id);