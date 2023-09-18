using Shared;

namespace Seatpicker.Domain;

public record User(UserId UserId, string Name);

public record UserId(string Id)
{
    public static implicit operator string(UserId id) => id.Id;
}