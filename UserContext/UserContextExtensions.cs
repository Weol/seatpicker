using Microsoft.Extensions.DependencyInjection;
using Seatpicker.Domain.Services;

namespace Seatpicker.Domain;

public static class UserContextExtensions
{
    public static IServiceCollection AddUserContext(this IServiceCollection services)
    {
        return services.AddUserRegistrationService();
    }
}