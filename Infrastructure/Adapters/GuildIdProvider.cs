namespace Seatpicker.Infrastructure.Adapters;

public class GuildIdProvider(IHttpContextAccessor httpContextAccessor)
{
    private string? guildId;

    private string? GetGuildIdFromHttpContext()
    {
        var httpContext = httpContextAccessor.HttpContext;
        return httpContext?.Request.RouteValues["guildId"]?.ToString();
    }

    public string GuildId
    {
        get => guildId ?? GetGuildIdFromHttpContext() ?? throw new NoGuildIdSetException();
        set => guildId = value;
    }
}

public class NoGuildIdSetException : Exception;