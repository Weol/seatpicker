namespace Seatpicker.Infrastructure.Authentication.Discord;

public record DiscordToken(
    string Id,
    string Nick,
    string? Avatar,
    string RefreshToken,
    Role[] Roles,
    string? GuildId);
