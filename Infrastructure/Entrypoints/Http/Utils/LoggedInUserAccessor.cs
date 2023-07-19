using Seatpicker.Domain;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Utils;

public interface ILoggedInUserAccessor
{
    User Get();
}

public class LoggedInUserAccessor : ILoggedInUserAccessor
{
    private readonly IHttpContextAccessor httpContextAccessor;

    public LoggedInUserAccessor(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    private HttpContext HttpContext =>
        httpContextAccessor.HttpContext ?? throw new Exception("HttpContext is null");

    public User Get()
    {
        var id  = HttpContext.User.Claims.First(x => x.Type == "spu_id").Value;
        var nick = HttpContext.User.Claims.First(x => x.Type == "spu_nick").Value;
        var avatar = HttpContext.User.Claims.First(x => x.Type == "spu_avatar").Value;

        return new User
        {
            Id = id,
            Avatar = avatar,
            Nick = nick,
        };
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
