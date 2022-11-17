using Microsoft.Extensions.DependencyInjection;
using Seatpicker.Domain.UserRegistration.Ports;

namespace Seatpicker.Domain.UserRegistration;

public interface IUserRegistrationService
{
    Task<User> Register(UnregisteredUser unregisteredUser, string password);
}

internal class UserRegistrationService : IUserRegistrationService
{
    private readonly IStoreUser storeUser;

    public UserRegistrationService(IStoreUser storeUser)
    {
        this.storeUser = storeUser;
    }

    public async Task<User> Register(UnregisteredUser unregisteredUser, string password)
    {
        var defaultClaims = new[]
        {
            Claim.Reserve,
            Claim.ViewReservedSeats
        };
        
        var user = new User(unregisteredUser.Email, unregisteredUser.Nick, unregisteredUser.Name, defaultClaims, DateTime.Now);

        await storeUser.Store(user);
        return user;
    }
}

internal static class UserRegistrationServiceExtensions
{
    public static IServiceCollection AddUserRegistrationService(this IServiceCollection services)
    {
        return services.AddSingleton<IUserRegistrationService, UserRegistrationService>();
    }
}