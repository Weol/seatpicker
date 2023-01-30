using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Seatpicker.Application.Features.Login;
using Seatpicker.Application.Features.Login.Ports;

namespace Seatpicker.Infrastructure.Adapters;

internal class DiscordClient : IDiscordAccessTokenProvider, IDiscordLookupUser
{
    private readonly HttpClient httpClient;
    private readonly JsonSerializerOptions jsonSerializerOptions;
    private readonly DiscordClientOptions options;
    private readonly ILogger<DiscordClient> logger;

    public DiscordClient(HttpClient httpClient, IOptions<DiscordClientOptions> options,
        JsonSerializerOptions jsonSerializerOptions, ILogger<DiscordClient> logger)
    {
        this.httpClient = httpClient;
        this.jsonSerializerOptions = jsonSerializerOptions;
        this.options = options.Value;
        this.logger = logger;
    }

    public async Task<string> GetFor(string discordToken)
    {
        var response = await httpClient.PostAsync("oauth2/token", new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("client_id", options.ClientId),
            new KeyValuePair<string, string>("client_secret", options.ClientSecret),
            new KeyValuePair<string, string>("redirect_uri", options.RedirectUri.ToString()),
            new KeyValuePair<string, string>("code", discordToken),
        }));

        var dto = await DeserializeContent<DiscordAccessTokenDto>(response);
        return dto.AccessToken;
    }

    public async Task<DiscordUser> Lookup(string accessToken)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, "users/@me");

        requestMessage.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await httpClient.SendAsync(requestMessage);
        return await DeserializeContent<DiscordUser>(response);
    }

    private async Task<TModel> DeserializeContent<TModel>(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        if (response.IsSuccessStatusCode)
        {
            logger.LogInformation("Discord API responded with code {StatusCode} and body {body}", response.StatusCode, body);
            
            return JsonSerializer.Deserialize<TModel>(body, jsonSerializerOptions)
                   ?? throw new NullReferenceException($"Could not deserialize Discord response to {nameof(TModel)}");
        }

        logger.LogError("Non-successful response code from Discord {@ResponseInfo}", new
        {
            response.StatusCode,
            Body = body,
        });

        throw new DiscordException($"Non-successful response code from Discord {response.StatusCode}");
    }

    private class DiscordAccessTokenDto
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = null!;

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
        
        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } = null!;

        [JsonPropertyName("scopes")]
        public IEnumerable<string> Scopes { get; set; } = null!;

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = null!;
    }
}

internal class DiscordException : Exception
{
    public Exception? Exception { get; set; }

    public DiscordException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public DiscordException(string message) : base(message)
    {
    }
}