using Microsoft.Extensions.DependencyInjection;
using Seatpicker.Domain.UserRegistration.Ports;

namespace Seatpicker.Domain.UserRegistration;

public interface IUserRegistrationService
{
    Task<User> Register(UnregisteredUser user, string password);
}

internal class UserRegistrationService : IUserRegistrationService
{
    private readonly IStoreUser storeUser;

    public UserRegistrationService(IStoreUser storeUser)
    {
        this.storeUser = storeUser;
    }

    public async Task<User> Register(UnregisteredUser user, string password)
    {
        return new User(id, user.EmailId, user.Nick, user.Name, claims, DateTime.Now);
    }
}

internal static class UserRegistrationServiceExtensions
{
    public static IServiceCollection AddUserRegistrationService(this IServiceCollection services)
    {
        return services.AddSingleton<IUserRegistrationService, UserRegistrationService>();
    }
}