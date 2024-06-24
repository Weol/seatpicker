using Marten;
using Seatpicker.Application.Features.Reservation;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Adapters.Database;
using Shared;

namespace Seatpicker.Infrastructure.Authentication;

#pragma warning disable CS1998
public class UserManager(IDocumentStore documentStore) : IUserProvider
{
    public async Task<User?> GetById(string userId)
    {
        await using var reader = documentStore.QuerySession();
        var userDocument = await reader.LoadAsync<UserDocument>(userId);

        return userDocument is null ? null : new User(userDocument.Id, userDocument.Name, userDocument.Avatar, userDocument.Roles);
    }

    public async Task<IEnumerable<User>> GetAll(string guildId)
    {
        await using var reader = documentStore.QuerySession(guildId);
        return reader.Query<UserDocument>()
            .Select(document => new User(document.Id, document.Name, document.Avatar, document.Roles));
    }

    public async Task Store(string guildId, User user)
    {
        await using var transaction = documentStore.LightweightSession(guildId);
        transaction.Store(new UserDocument(user.Id, user.Name, user.Avatar, user.Roles));
        await transaction.SaveChangesAsync();
    }

    public record UserDocument(string Id, string Name, string? Avatar, IEnumerable<Role> Roles) : IDocument;
}

public static class UserManagerExtensions
{
    public static IServiceCollection AddUserManager(this IServiceCollection services)
    {
        return services
            .AddPortMapping<IUserProvider, UserManager>();
    }
}