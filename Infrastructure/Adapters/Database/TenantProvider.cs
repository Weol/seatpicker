using Seatpicker.Infrastructure.Authentication;
using Seatpicker.Infrastructure.Entrypoints.Utils;

namespace Seatpicker.Infrastructure.Adapters.Database;

public interface ITenantProvider
{
    public string GetTenant();
}

public class TenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor httpContextAccessor;

    public TenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    public string GetTenant()
    {
        var context = httpContextAccessor.HttpContext;
        if (context is null) throw new HttpContextIsNullException();

        var feature = context.Features.Get<TenantIdFeature>();
        return feature?.TenantId ?? throw new TenantIdMissingException();
    }

    public class HttpContextIsNullException : Exception
    {
    }

    public class TenantIdMissingException : Exception
    {
    }
}