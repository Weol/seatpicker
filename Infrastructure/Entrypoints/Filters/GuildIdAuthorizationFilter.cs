using Seatpicker.Domain;
using Seatpicker.Infrastructure.Adapters;
using Seatpicker.Infrastructure.Authentication;

namespace Seatpicker.Infrastructure.Entrypoints.Filters;

public class GuildIdAuthorizationFilter(GuildIdProvider guildIdProvider) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (context.HttpContext.GetRouteValue("guildId") is not string guildId) throw new GuildIdMissingException();

        if (context.HttpContext.User.Identity?.IsAuthenticated == true)
        {
            var claimGuildId = context.HttpContext.User.Claims
                .FirstOrDefault(claim => claim.Type == JwtTokenCreator.GuildIdClaimName);

            var isSuperadmin = context.HttpContext.User.IsInRole(Role.Superadmin.ToString());
            if (claimGuildId is null && !isSuperadmin)
                return Results.Forbid();

            if (claimGuildId is not null && guildId != claimGuildId.Value)
                return Results.Forbid();
        }

        guildIdProvider.GuildId = guildId;

        return await next(context);
    }

    public class GuildIdMissingException : Exception
    {

    }
}