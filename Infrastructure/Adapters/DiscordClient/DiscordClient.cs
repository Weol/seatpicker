﻿using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Seatpicker.Infrastructure.Entrypoints.Http;

namespace Seatpicker.Infrastructure.Adapters.DiscordClient;

public class DiscordClient
{
    private readonly HttpClient httpClient;
    private readonly JsonSerializerOptions jsonSerializerOptions;
    private readonly DiscordClientOptions options;
    private readonly ILogger<DiscordClient> logger;

    public DiscordClient(
        HttpClient httpClient,
        IOptions<DiscordClientOptions> options,
        JsonSerializerOptions jsonSerializerOptions,
        ILogger<DiscordClient> logger)
    {
        this.httpClient = httpClient;
        this.jsonSerializerOptions = jsonSerializerOptions;
        this.options = options.Value;
        this.logger = logger;
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

    private async Task<TModel> DeserializeContent<TModel>(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
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
}

public class DiscordAccessToken
{
    [JsonPropertyName("access_token")] public string AccessToken { get; set; } = null!;

    [JsonPropertyName("expires_in")] public int ExpiresIn { get; set; }

    [JsonPropertyName("refresh_token")] public string RefreshToken { get; set; } = null!;

    [JsonPropertyName("scopes")] public IEnumerable<string> Scopes { get; set; } = null!;

    [JsonPropertyName("token_type")] public string TokenType { get; set; } = null!;
}

internal class DiscordException : Exception
{
    public required HttpStatusCode StatusCode { get; init; }

    public required string Body { get; init; }

    public DiscordException(string message) : base(message)
    {
    }
}