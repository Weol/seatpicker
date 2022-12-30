using System.Text.Json.Serialization;

namespace Seatpicker.UserContext.Application.UserToken;

public record DiscordAccessToken(
    [property: JsonPropertyName("access_token")] string AccessToken, 
    [property: JsonPropertyName("expires_in")] int ExpiresIn,
    [property: JsonPropertyName("refresh_token")] string RefreshToken,
    [property: JsonPropertyName("scopes")] IEnumerable<string> Scopes,
    [property: JsonPropertyName("token_type")] string TokenType);

public record DiscordUser(
    string Id, 
    string Username,
    string Avatar,
    string Discriminator);