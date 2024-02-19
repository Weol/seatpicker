using System.Text.Json.Serialization;

namespace Seatpicker.Infrastructure.Adapters.Discord;

public class DiscordUser
{
    [JsonPropertyName("id")] public string Id { get; set; } = null!;

    [JsonPropertyName("username")] public string Username { get; set; }

    [JsonPropertyName("avatar")] public string? Avatar { get; set; } = null!;
}

public class DiscordAccessToken
{
    [JsonPropertyName("access_token")] public string AccessToken { get; set; } = null!;

    [JsonPropertyName("expires_in")] public int ExpiresIn { get; set; }

    [JsonPropertyName("refresh_token")] public string RefreshToken { get; set; } = null!;

    [JsonPropertyName("scopes")] public IEnumerable<string> Scopes { get; set; } = null!;

    [JsonPropertyName("token_type")] public string TokenType { get; set; } = null!;
}

public class DiscordGuildMember
{
    [JsonPropertyName("user")] public DiscordUser DiscordUser { get; set; } = null!;

    [JsonPropertyName("nick")] public string? Nick { get; set; } = null!;

    [JsonPropertyName("avatar")] public string? Avatar { get; set; } = null!;

    [JsonPropertyName("roles")] public IEnumerable<string> Roles { get; set; } = null!;
}

public class DiscordGuildRole
{
    [JsonPropertyName("id")] public string Id { get; set; } = null!;

    [JsonPropertyName("name")] public string Name { get; set; } = null!;

    [JsonPropertyName("color")] public int Color { get; set; }

    [JsonPropertyName("icon")] public string? Icon { get; set; } = null!;
}

public class DiscordGuild
{
    [JsonPropertyName("id")] public string Id { get; set; } = null!;

    [JsonPropertyName("name")] public string Name { get; set; } = null!;

    [JsonPropertyName("icon")] public string? Icon { get; set; } = null!;
}