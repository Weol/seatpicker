namespace Seatpicker.Domain;

public record User(string Id, string Name, string? Avatar, IEnumerable<Role> Roles);

public enum Role
{
    Superadmin,
    Admin,
    Operator,
    User,
}
