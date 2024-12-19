using Seatpicker.Application.Features;
using Seatpicker.Application.Features.Reservation;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Authentication;

namespace Seatpicker.Infrastructure.Adapters;

public class UserProvider(IDocumentReader documentReader) : IUserProvider
{
    public async Task<User?> GetById(string userId)
    {
        var userDocument = await documentReader.Query<UserDocument>(userId);

        return userDocument is null ? null : new User(userDocument.Id, userDocument.Name, userDocument.Avatar, userDocument.Roles);
    }
}