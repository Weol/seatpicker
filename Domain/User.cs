namespace Seatpicker.Domain;

public class User
{
    public User(string id, string nick, string avatar)
    {
        Id = id;
        Nick = nick;
        Avatar = avatar;
    }

    public string Id { get; init; }

    public string Nick { get; init; }

    public string Avatar { get; init; }
}