using System.Text.Json.Serialization;

namespace Seatpicker.Infrastructure.Adapters.Discord;

public record DiscordUser(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("global_name")] string? GlobalName,
    [property: JsonPropertyName("avatar")] string? Avatar);

public record DiscordAccessToken(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("expires_in")] int ExpiresIn,
    [property: JsonPropertyName("refresh_token")] string RefreshToken);

public record DiscordGuildMember(
    [property: JsonPropertyName("user")] DiscordUser DiscordUser,
    [property: JsonPropertyName("nick")] string? Nick,
    [property: JsonPropertyName("avatar")] string? Avatar,
    [property: JsonPropertyName("roles")] IEnumerable<string> Roles);

public record DiscordGuild(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("icon")] string? Icon,
    [property: JsonPropertyName("roles")] DiscordGuildRole[] Roles);

public record DiscordGuildRole(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("color")] int Color,
    [property: JsonPropertyName("icon")] string? Icon);
