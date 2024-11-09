using Seatpicker.Domain;
using Seatpicker.Infrastructure.Adapters.Database;
using Shared;

namespace Seatpicker.Infrastructure.Authentication;

#pragma warning disable CS1998
public class UserManager(DocumentRepository documentRepository)
{
    public async Task<User?> GetById(string? guildId, string userId)
    {
        using var reader = guildId is null
            ? documentRepository.CreateGuildlessReader()
            : documentRepository.CreateReader(guildId);

        var userDocument = await reader.Query<UserDocument>(userId);

        return userDocument is null ? null : new User(userDocument.Id, userDocument.Name, userDocument.Avatar, userDocument.Roles);
    }

    public async Task<IEnumerable<User>> GetAll(string? guildId)
    {
        using var reader = guildId is null
            ? documentRepository.CreateGuildlessReader()
            : documentRepository.CreateReader(guildId);

        return reader.Query<UserDocument>()
            .Select(document => new User(document.Id, document.Name, document.Avatar, document.Roles));
    }

    public async Task Store(string? guildId, User user)
    {
        using var transaction = guildId is null
            ? documentRepository.CreateGuildlessTransaction()
            : documentRepository.CreateTransaction(guildId);

        transaction.Store(new UserDocument(user.Id, user.Name, user.Avatar, user.Roles));
        await transaction.Commit();
    }

    public record UserDocument(string Id, string Name, string? Avatar, IEnumerable<Role> Roles) : IDocument;
}

public static class UserManagerExtensions
{
    public static IServiceCollection AddUserManager(this IServiceCollection services)
    {
        return services
            .AddScoped<UserManager>();
    }
}