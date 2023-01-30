using System.Net;
using Bogus;
using Microsoft.Extensions.DependencyInjection;
using Seatpicker.Application.Features.Login;

namespace Seatpicker.IntegrationTests.Host.Adapters;

public class DiscordClientFaker
{
    private readonly HttpRequestFaker httpRequestFaker;
    private readonly Host host;

    public DiscordClientFaker(HttpRequestFaker httpRequestFaker, Host host)
    {
        this.httpRequestFaker = httpRequestFaker;
        this.host = host;
    }

    public void SetupDiscordUser(string accessToken, string loginToken, DiscordUser discordUser)
    {
        var accessTokenUri = new Uri("/ouath2/token");
        httpRequestFaker.WhenUri(accessTokenUri, request =>
        {
            var formContent = (FormUrlEncodedContent)request.Content!;
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(CreateAccessTokenResponseBody()),
            };
        });
        
        var usersUri = new Uri("users/@me");
        httpRequestFaker.WhenUri(usersUri, request =>
        {
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(CreateDiscordUserResponseBody()),
            };
        });
    }

    private string CreateAccessTokenResponseBody()
    {
        var accessToken = new Faker().Random.Hexadecimal(31, "");
        return $@"{{
            ""access_token"": ""{accessToken}"",
            ""token_type"": ""Bearer"",
            ""expires_in"": 604800,
            ""refresh_token"": ""D43f5y0ahjqew82jZ4NViEr2YafMKhue"",
            ""scope"": ""identify""
        }}";
    }
    private string CreateDiscordUserResponseBody()
    {
        return @"{
           ""id"": ""${}"",
           ""username"": ""Nelly"",
           ""discriminator"": ""1337"",
           ""avatar"": ""8342729096ea3675442027381ff50dfe"",
           ""verified"": true,
           ""email"": ""nelly@discord.com"",
           ""flags"": 64,
           ""banner"": ""06c16474723fe537c283b8efa61a30c8"",
           ""accent_color"": 16711680,
           ""premium_type"": 1,
           ""public_flags"": 64
       }";
    }
}

public static class DiscordFakerHostExtensions
{
    public static IServiceCollection AddDiscordClientFaker(this IServiceCollection services)
    {
        return services.AddSingleton<DiscordClientFaker>();
    }
}