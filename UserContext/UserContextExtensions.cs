using Microsoft.Extensions.DependencyInjection;
using Seatpicker.Domain.Registration;

namespace Seatpicker.Domain;

public static class UserContextExtensions
{
    public static IServiceCollection AddUserContext(this IServiceCollection services)
    {
        return services.AddUserRegistrationService();
    }
}