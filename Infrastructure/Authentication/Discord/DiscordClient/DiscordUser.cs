namespace Seatpicker.Infrastructure.Authentication.Discord.DiscordClient;

public record DiscordUser(
    string Id,
    string Username,
    string? Avatar);