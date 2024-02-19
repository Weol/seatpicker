using Seatpicker.Domain;

namespace Seatpicker.Infrastructure.Authentication;

public record DiscordToken(
    string Id,
    string Nick,
    string? Avatar,
    string RefreshToken,
    Role[] Roles,
    string GuildId,
    AuthenticationProvider Provider);