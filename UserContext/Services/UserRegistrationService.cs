using Microsoft.Extensions.DependencyInjection;

namespace Seatpicker.Domain.Services;

public interface IUserRegistrationService
{
    Task<User> Register(UnregisteredUser user, string password);
}

internal class UserRegistrationService : IUserRegistrationService
{
    public Task<User> Register(UnregisteredUser user, string password)
    {
        throw new NotImplementedException();
    }
}

internal static class UserRegistrationServiceExtensions
{
    public static IServiceCollection AddUserRegistrationService(this IServiceCollection services)
    {
        return services.AddSingleton<IUserRegistrationService, UserRegistrationService>();
    }
}