using Microsoft.Extensions.DependencyInjection;
using Seatpicker.Domain.Application.UserToken;
using Seatpicker.Domain.Domain.Registration;

namespace Seatpicker.Domain;

public interface IUserContextConfiguration
{
    string ClientId { get; }
}

public static class UserContextExtensions
{
    private static IUserContextConfiguration Configuration { get; set; } = null!;

    public static IServiceCollection AddUserContext(this IServiceCollection services, IUserContextConfiguration configuration)
    {
        Configuration = configuration;
        
        return services
            .AddUserTokenService(ConfigureUserTokenService)
            .AddLoginService();
    }

    private static void ConfigureUserTokenService(UserTokenService.Options options)
    {
        options.ClientId = Configuration.ClientId;
    }
}