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
        if (context is null) throw new NullReferenceException();

        var header = context.Request.Headers["Seatpicker-Tenant"];
        return header[0] ?? throw new NullReferenceException();
    }
}