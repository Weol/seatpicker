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

    public async Task<User?> GetById(UserId userId)
    {
        using var reader = documentRepository.CreateReader();
        var userDocument = await reader.Get<UserDocument>(userId);

        return userDocument is null ? null : new User(new UserId(userDocument.Id), userDocument.Name, userDocument.Avatar);
    }

    public async Task<IEnumerable<User>> GetAllInGuild(string guildId)
    {
        using var reader = documentRepository.CreateReader();
        return reader.Query<UserDocument>()
            .Where(user => user.GuildId == guildId)
            .Select(document => new User(new UserId(document.Id), document.Name, document.Avatar));
    }

    public async Task Store(User user, string guildId)
    {
        using var transaction = documentRepository.CreateTransaction();
        transaction.Store(new UserDocument(user.Id, guildId, user.Name, user.Avatar));
        await transaction.Commit();
    }

    public record UserDocument(string Id, string GuildId, string Name, string? Avatar) : IDocument;
}

public static class UserManagerExtensions
{
    public static IServiceCollection AddUserManager(this IServiceCollection services)
    {
        return services.AddPortMapping<IUserProvider, UserManager>();
    }
}