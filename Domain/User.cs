namespace Seatpicker.Domain;

public record User(UserId Id, string Name, string? Avatar);

public record UserId(string Value)
{
    public static implicit operator string(UserId id) => id.Value;
}