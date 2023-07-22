namespace Seatpicker.Domain;

public class User
{
    public string Id { get; init; }

    public string Nick { get; init; }

    public string? Avatar { get; init; }

    public override string ToString() => $"User {Nick} ({Id})";
}