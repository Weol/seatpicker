using Seatpicker.Infrastructure.Authentication;

namespace Seatpicker.Infrastructure.Entrypoints.Filters;

public class GuildIdAuthorizationFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (context.HttpContext.GetRouteValue("guildId") is not string guildId) throw new GuildIdMissingException();
        
        if (context.HttpContext.User.Identity?.IsAuthenticated == true)
        {
            var claimGuildId = context.HttpContext.User.Claims
                .First(claim => claim.Type == JwtTokenCreator.GuildIdClaimName)
                .Value;

            if (guildId != claimGuildId)
            {
                context.HttpContext.Response.StatusCode = 403;
            }
        }

        context.HttpContext.Features.Set(new GuildIdFeature(guildId));
        
        return await next(context);
    }

    public class GuildIdMissingException : Exception
    {
        
    }
}

record GuildIdFeature(string GuildId);