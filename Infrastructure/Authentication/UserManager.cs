using Seatpicker.Application.Features;
using Seatpicker.Application.Features.Seats;
using Seatpicker.Domain;
using Shared;

namespace Seatpicker.Infrastructure.Authentication;

#pragma warning disable CS1998
public class UserManager : IUserProvider
{
    private readonly IDocumentRepository documentRepository;

    public UserManager(IDocumentRepository documentRepository)
    {
        this.documentRepository = documentRepository;
    }

    public async Task<User?> GetById(string userId)
    {
        using var reader = documentRepository.CreateReader();
        var userDocument = await reader.Query<UserDocument>(userId);

        return userDocument is null ? null : new User(userDocument.Id, userDocument.Name, userDocument.Avatar, userDocument.Roles);
    }

    public async Task<IEnumerable<User>> GetAll()
    {
        using var reader = documentRepository.CreateReader();
        return reader.Query<UserDocument>()
            .Select(document => new User(document.Id, document.Name, document.Avatar, document.Roles));
    }

    public async Task Store(string guildId, User user)
    {
        using var transaction = documentRepository.CreateTransaction(guildId);
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
            .AddPortMapping<IUserProvider, UserManager>();
    }
}