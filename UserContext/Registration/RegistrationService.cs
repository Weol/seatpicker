using Microsoft.Extensions.DependencyInjection;
using Seatpicker.Domain.Registration.Ports;

namespace Seatpicker.Domain.Registration;

public interface IRegistrationService
{
    Task<User> Register(UnregisteredUser unregisteredUser);
}

internal class RegistrationService : IRegistrationService
{
    private readonly IStoreUser storeUser;

    public RegistrationService(IStoreUser storeUser)
    {
        this.storeUser = storeUser;
    }

    public async Task<User> Register(UnregisteredUser unregisteredUser)
    {
        var defaultClaims = new[]
        {
            Role.SeatReserver,
            Role.ReservedSeatsViewer
        };
        
        var user = new User(
            unregisteredUser.Id, 
            unregisteredUser.Nick, 
            unregisteredUser.Avatar, 
            unregisteredUser.Name, 
            defaultClaims, 
            DateTime.Now);

        await storeUser.Store(user);
        return user;
    }
}

internal static class UserRegistrationServiceExtensions
{
    public static IServiceCollection AddUserRegistrationService(this IServiceCollection services)
    {
        return services.AddSingleton<IRegistrationService, RegistrationService>();
    }
}