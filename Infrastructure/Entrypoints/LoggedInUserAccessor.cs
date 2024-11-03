using System.Security.Claims;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Authentication;

namespace Seatpicker.Infrastructure.Entrypoints;

public interface ILoggedInUserAccessor
{
    Task<User> GetUser();
}

public class LoggedInUserAccessor(IHttpContextAccessor httpContextAccessor, UserManager userManager)
    : ILoggedInUserAccessor
{
    private HttpContext HttpContext =>
        httpContextAccessor.HttpContext ?? throw new HttpContextIsNullException();

    public async Task<User> GetUser()
    {
        var id  = HttpContext.User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
        var guildId  = HttpContext.User.Claims.FirstOrDefault(x => x.Type == JwtTokenCreator.GuildIdClaimName)?.Value;

        return await userManager.GetById(guildId, id) ??
               throw new UserNotFoundException($"Cannot find user with id {id}");
    }

    public class HttpContextIsNullException : Exception
    {
    }

    public class UserNotFoundException(string message) : Exception(message);
}

public static class LoggedInUserAccessorExtensions
{
    public static IServiceCollection AddLoggedInUserAccessor(this IServiceCollection services)
    {
        return services
            .AddHttpContextAccessor()
            .AddScoped<ILoggedInUserAccessor, LoggedInUserAccessor>();
    }
}
