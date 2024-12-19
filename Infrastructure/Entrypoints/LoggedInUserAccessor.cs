using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using JasperFx.Core.Reflection;
using Seatpicker.Application.Features;
using Seatpicker.Application.Features.Lan;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Authentication;

namespace Seatpicker.Infrastructure.Entrypoints;

public interface ILoggedInUserAccessor
{
    Task<User> GetUser();
}

public class LoggedInUserAccessor(IHttpContextAccessor httpContextAccessor, IDocumentReader documentReader)
    : ILoggedInUserAccessor
{
    private HttpContext HttpContext =>
        httpContextAccessor.HttpContext ?? throw new HttpContextIsNullException();

    public async Task<User> GetUser()
    {
        var id  = HttpContext.User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
        var guildId  = HttpContext.User.Claims.FirstOrDefault(x => x.Type == JwtTokenCreator.GuildIdClaimName)?.Value;
        var roles = HttpContext.User.Claims
            .Where(x => x.Type == ClaimTypes.Role)
            .Select(x => Enum.Parse<Role>(x.Value));

        if (guildId is null)
        {
            var nick  = HttpContext.User.Claims.First(x => x.Type == JwtTokenCreator.NickClaimName).Value;
            var avatar = HttpContext.User.Claims.FirstOrDefault(x => x?.Type == JwtTokenCreator.IdClaimName, null)?.Value;
            return new User(id, nick, avatar, roles);
        }

        var userDocument = await documentReader.Query<UserDocument>(id) ??
               throw new UserNotFoundException{ UserId = id };

        return new User(userDocument.Id, userDocument.Name, userDocument.Avatar, userDocument.Roles);
    }

    public class HttpContextIsNullException : Exception;

    public class UserNotFoundException : Exception
    {
        public required string UserId { get; init; }
    }
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
