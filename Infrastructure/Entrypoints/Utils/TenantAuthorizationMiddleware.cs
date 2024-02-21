using Seatpicker.Domain;
using Seatpicker.Infrastructure.Authentication;

namespace Seatpicker.Infrastructure.Entrypoints.Utils;

public class TenantAuthorizationMiddleware
{
    public const string TenantHeaderName = "Tenant-Id";

    private readonly RequestDelegate next;

    public TenantAuthorizationMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var tenantHeaders = context.Request.Headers[TenantHeaderName];
        if (tenantHeaders.Count != 1)
        {
            context.Response.StatusCode = 403;
            return;
        }

        var tenant = tenantHeaders.FirstOrDefault();
        if (tenant is null)
        {
            context.Response.StatusCode = 400;
            return;
        }

        if (context.User.Identity?.IsAuthenticated == true)
        {
            var isSuperadmin = context.User.IsInRole(Role.Superadmin.ToString());

            if (!isSuperadmin)
            {
                var claimTenant = context.User.Claims
                    .First(claim => claim.Type == "tenant_id")
                    .Value;

                if (tenant != claimTenant)
                {
                    context.Response.StatusCode = 403;
                    return;
                }
            }
        }

        context.Features.Set(new TenantIdFeature(tenant));

        await next(context);
    }
}

record TenantIdFeature(string TenantId);