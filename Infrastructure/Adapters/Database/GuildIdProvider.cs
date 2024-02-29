using Seatpicker.Infrastructure.Entrypoints.Filters;

namespace Seatpicker.Infrastructure.Adapters.Database;

public class GuildIdProvider
{
    private readonly IHttpContextAccessor httpContextAccessor;

    public GuildIdProvider(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    public string GetGuildId()
    {
        var context = httpContextAccessor.HttpContext;
        if (context is null) throw new HttpContextIsNullException();

        var feature = context.Features.Get<GuildIdFeature>();
        return feature?.GuildId ?? throw new GuildIdMissingException();
    }

    public void SetGuildId(string guildId)
    {
        var context = httpContextAccessor.HttpContext;
        if (context is null) throw new HttpContextIsNullException();

        context.Features.Set(new GuildIdFeature(guildId));
    }

    public class HttpContextIsNullException : Exception
    {
    }

    public class GuildIdMissingException : Exception
    {
    }
}