using Seatpicker.Domain;
using Shared;

namespace Seatpicker.Infrastructure.Authentication;

public record UserDocument(string Id, string Name, string? Avatar, IEnumerable<Role> Roles) : IDocument;