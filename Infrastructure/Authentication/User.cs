using Shared;

namespace Seatpicker.Infrastructure.Authentication.UserStore;

public class User : IDocument
{
    public string Id { get; set; }

    public string Name { get; set; }

    public ICollection<string> Roles { get; set; }

    public DateTimeOffset InitialLoginTime { get; set; }

    public DateTimeOffset LastLoginTime { get; set; }
}