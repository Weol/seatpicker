using System.Security.Claims;
using Seatpicker.Application.Features.Seats;
using Seatpicker.Domain;

namespace Seatpicker.Infrastructure.Entrypoints;

public interface ILoggedInUserAccessor
{
    Task<User> GetUser();
}

public class LoggedInUserAccessor : ILoggedInUserAccessor
{
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly IUserProvider userProvider;

    public LoggedInUserAccessor(IHttpContextAccessor httpContextAccessor, IUserProvider userProvider)
    {
        this.httpContextAccessor = httpContextAccessor;
        this.userProvider = userProvider;
    }

    private HttpContext HttpContext =>
        httpContextAccessor.HttpContext ?? throw new HttpContextIsNullException();

    public async Task<User> GetUser()
    {
        var id  = HttpContext.User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;

        return await userProvider.GetById(id) ??
               throw new UserNotFoundException($"Cannot find user with id {id}");
    }

    public class HttpContextIsNullException : Exception
    {
    }

    public class UserNotFoundException : Exception
    {
        public UserNotFoundException(string message) : base(message)
        {
        }
    }
}

public static class LoggedInUserAccessorExtensions
{
    public static IServiceCollection AddLoggedInUserAccessor(this IServiceCollection services)
    {
        return services
            .AddHttpContextAccessor()
            .AddTransient<ILoggedInUserAccessor, LoggedInUserAccessor>();
    }
}
