using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace Seatpicker.Infrastructure.Authentication.Discord.DiscordClient;

public class DiscordClient
{
    private readonly HttpClient httpClient;
    private readonly JsonSerializerOptions jsonSerializerOptions;
    private readonly DiscordClientOptions options;
    private readonly ILogger<DiscordClient> logger;
    private readonly IMemoryCache memoryCache;

    public DiscordClient(
        HttpClient httpClient,
        IOptions<DiscordClientOptions> options,
        JsonSerializerOptions jsonSerializerOptions,
        ILogger<DiscordClient> logger,
        IMemoryCache memoryCache)
    {
        this.httpClient = httpClient;
        this.jsonSerializerOptions = jsonSerializerOptions;
        this.options = options.Value;
        this.logger = logger;
        this.memoryCache = memoryCache;
    }

    public async Task<DiscordAccessToken> GetAccessToken(string discordToken)
    {
        var response = await httpClient.PostAsync(
            "oauth2/token",
            new FormUrlEncodedContent(
                new[]
                {
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("client_id", options.ClientId),
                    new KeyValuePair<string, string>("client_secret", options.ClientSecret),
                    new KeyValuePair<string, string>("redirect_uri", options.RedirectUri.ToString()),
                    new KeyValuePair<string, string>("code", discordToken),
                }));

        return await DeserializeContent<DiscordAccessToken>(response);
    }

    public async Task<DiscordAccessToken> RefreshAccessToken(string refreshToken)
    {
        var response = await httpClient.PostAsync(
            "oauth2/token",
            new FormUrlEncodedContent(
                new[]
                {
                    new KeyValuePair<string, string>("grant_type", "refresh_token"),
                    new KeyValuePair<string, string>("client_id", options.ClientId),
                    new KeyValuePair<string, string>("client_secret", options.ClientSecret),
                    new KeyValuePair<string, string>("refresh_token", refreshToken),
                }));

        return await DeserializeContent<DiscordAccessToken>(response);
    }

    public async Task<DiscordUser> Lookup(string accessToken)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, "users/@me");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await httpClient.SendAsync(requestMessage);
        return await DeserializeContent<DiscordUser>(response);
    }

    public async Task<GuildMember?> GetGuildMember(string guildId, string memberId)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"guilds/{guildId}/members/{memberId}");

        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bot", options.BotToken);

        var response = await httpClient.SendAsync(requestMessage);
        try
        {
            return await DeserializeContent<GuildMember>(response);
        }
        catch (DiscordException e) when (e.StatusCode == HttpStatusCode.NotFound)
        {
            logger.LogInformation("Could not find member with id {MemberId} in guild {GuildId}", memberId, guildId);
            return null;
        }
    }

    public async Task<IEnumerable<GuildRole>> GetGuildRoles(string guildId)
    {
        return await MakeCachedRequest($"guild_roles_{guildId}", async () =>
        {
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"guilds/{guildId}/roles");

            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bot", options.BotToken);

            var response = await httpClient.SendAsync(requestMessage);
            return await DeserializeContent<GuildRole[]>(response);
        });
    }

    public async Task<IEnumerable<Guild>> GetGuilds()
    {
        return await MakeCachedRequest("get_bot_guilds", async () =>
        {
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, "users/@me/guilds");

            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bot", options.BotToken);

            var response = await httpClient.SendAsync(requestMessage);
            return await DeserializeContent<IEnumerable<Guild>>(response);
        });
    }

    private async Task<TResponse> MakeCachedRequest<TResponse>(string cacheKey,
        Func<Task<TResponse>> requestFunc)
        where TResponse : notnull
    {
        
        if (memoryCache.TryGetValue(cacheKey, out TResponse? cachedResponse))
        {
            if (cachedResponse is not null)
            {
                logger.LogInformation("Returning cached payload for cache key {CacheKey}", cacheKey);
                return cachedResponse;
            }
        }

        var response = await requestFunc();
        memoryCache.Set(cacheKey, response, TimeSpan.FromMinutes(5));
        return response;
    }

    private async Task<TModel> DeserializeContent<TModel>(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        LogRateLimit(response);

        if (response.IsSuccessStatusCode)
        {
            logger.LogInformation(
                "Discord API responded with code {StatusCode} and body {Body}",
                response.StatusCode,
                body);

            return JsonSerializer.Deserialize<TModel>(body, jsonSerializerOptions) ??
                throw new NullReferenceException($"Could not deserialize Discord response to {nameof(TModel)}");
        }

        logger.LogError(
            "Non-successful response code from Discord {@ResponseInfo}",
            new { response.StatusCode, Body = body, });

        throw new DiscordException($"Non-successful response code from Discord {response.StatusCode}")
        {
            StatusCode = response.StatusCode,
            Body = body,
        };
    }

    private void LogRateLimit(HttpResponseMessage response)
    {
        response.Headers.TryGetValues("X-RateLimit-Limit", out var limit);
        response.Headers.TryGetValues("X-RateLimit-Remaining", out var remaining);
        response.Headers.TryGetValues("X-RateLimit-Reset", out var reset);
        response.Headers.TryGetValues("X-RateLimit-Reset-After", out var resetAfter);
        response.Headers.TryGetValues("X-RateLimit-Bucket", out var bucket);
        response.Headers.TryGetValues("X-RateLimit-Global", out var global);
        response.Headers.TryGetValues("X-RateLimit-Scope", out var scope);

        var rateLimit = new DiscordRateLimit(
            limit?.FirstOrDefault(),
            remaining?.FirstOrDefault(),
            reset?.FirstOrDefault(),
            resetAfter?.FirstOrDefault(),
            bucket?.FirstOrDefault(),
            global?.FirstOrDefault(),
            scope?.FirstOrDefault()
        );

        logger.LogInformation("Discord rate limit: {@RateLimit}", rateLimit);
    }
}

public record DiscordRateLimit(string? Limit, string? Remaining, string? Reset, string? ResetAfter, string? Bucket,
    string? Global,
    string? Scope);

public class DiscordAccessToken
{
    [JsonPropertyName("access_token")] public string AccessToken { get; set; } = null!;

    [JsonPropertyName("expires_in")] public int ExpiresIn { get; set; }

    [JsonPropertyName("refresh_token")] public string RefreshToken { get; set; } = null!;

    [JsonPropertyName("scopes")] public IEnumerable<string> Scopes { get; set; } = null!;

    [JsonPropertyName("token_type")] public string TokenType { get; set; } = null!;
}

public class GuildMember
{
    [JsonPropertyName("user")] public DiscordUser DiscordUser { get; set; } = null!;

    [JsonPropertyName("nick")] public string Nick { get; set; } = null!;

    [JsonPropertyName("avatar")] public string Avatar { get; set; } = null!;

    [JsonPropertyName("roles")] public IEnumerable<string> Roles { get; set; } = null!;
}

public class GuildRole
{
    [JsonPropertyName("id")] public string Id { get; set; } = null!;

    [JsonPropertyName("name")] public string Name { get; set; } = null!;

    [JsonPropertyName("color")] public int Color { get; set; }

    [JsonPropertyName("icon")] public string? Icon { get; set; } = null!;
}

public class Guild
{
    [JsonPropertyName("id")] public string Id { get; set; } = null!;

    [JsonPropertyName("name")] public string Name { get; set; } = null!;

    [JsonPropertyName("icon")] public string? Icon { get; set; } = null!;
}

internal class DiscordException : Exception
{
    public required HttpStatusCode StatusCode { get; init; }

    public required string Body { get; init; }

    public DiscordException(string message) : base(message)
    {
    }
}