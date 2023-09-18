using Seatpicker.Application.Features;
using Seatpicker.Application.Features.Seats;
using Seatpicker.Domain;

namespace Seatpicker.Infrastructure.Adapters;

public class UserProvider : IUserProvider
{
    private readonly IDocumentReader documentReader;

    public UserProvider(IDocumentReader documentReader)
    {
        this.documentReader = documentReader;
    }

    public Task<User?> GetById(UserId userId)
    {
        return Task.FromResult<User>(null);
    }
}

public static class UserProviderExtensions
{
    public static IServiceCollection AddUserProvider(this IServiceCollection services)
    {
        return services.AddScoped<IUserProvider, UserProvider>();
    }
}