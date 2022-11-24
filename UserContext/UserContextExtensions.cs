using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Seatpicker.Domain.Application.UserToken;
using Seatpicker.Domain.Domain.Registration;

namespace Seatpicker.Domain;

public static class UserContextExtensions
{
    private static IConfiguration Configuration { get; set; } = null!;

    public static IServiceCollection AddUserContext(this IServiceCollection services, IConfiguration configuration)
    {
        Configuration = configuration;
        
        return services
            .AddUserTokenService(ConfigureUserTokenService)
            .AddLoginService();
    }

    private static void ConfigureUserTokenService(UserTokenService.Options options)
    {
        options.ClientId = Configuration["ClientId"];
    }
}