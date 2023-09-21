using Seatpicker.Application.Features;
using Seatpicker.Application.Features.Seats;
using Seatpicker.Domain;
using Shared;

namespace Seatpicker.Infrastructure.Authentication;

public class UserManager : IUserProvider
{
    private readonly IDocumentRepository documentRepository;

    public UserManager(IDocumentRepository documentRepository)
    {
        this.documentRepository = documentRepository;
    }

    public async Task<User?> GetById(UserId userId)
    {
        var reader = documentRepository.CreateReader();
        var userDocument = await reader.Get<UserDocument>(userId);

        return userDocument is null ? null : new User(new UserId(userDocument.Id), userDocument.Name);
    }

    public async Task Store(User user)
    {
        await using var transaction = documentRepository.CreateTransaction();
        transaction.Store(new UserDocument(user.Id, user.Name));
        transaction.Commit();
    }

    public record UserDocument(string Id, string Name) : IDocument;
}

public static class UserManagerExtensions
{
    public static IServiceCollection AddUserManager(this IServiceCollection services)
    {
        return services.AddPortMapping<IUserProvider, UserManager>();
    }
}