namespace Seatpicker.Infrastructure.Entrypoints.Http;

public record User(string Id, string Name, string? Avatar)
{
    public static User FromDomainUser(Domain.User user)
    {
        return new User(user.Id, user.Name, user.Avatar);
    }
}