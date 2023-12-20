using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Seatpicker.Infrastructure.Adapters.Database.GuildRoleMapping;
using Seatpicker.Infrastructure.Authentication;
using Seatpicker.Infrastructure.Authentication.Discord.DiscordClient;
using Seatpicker.Infrastructure.Entrypoints.Http.Authentication.Discord;
using Seatpicker.IntegrationTests.HttpInterceptor.Discord;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.Authentication;

// ReSharper disable once InconsistentNaming
public class Login : LoginAndRenewBase
{
    public Login(TestWebApplicationFactory factory,
        PostgresFixture databaseFixture,
        ITestOutputHelper testOutputHelper) : base(
        factory,
        databaseFixture,
        testOutputHelper)
    {
    }

    protected override Task<HttpResponseMessage> MakeRequest(HttpClient client)
    {
        return client.PostAsync(
            "authentication/discord/login",
            JsonContent.Create(new LoginEndpoint.Request("token", GuildId, "https://local.host")));
    }

    [Theory]
    [InlineData("Tore XD Tang", "1231289378")]
    [InlineData(null, "1231289378")]
    [InlineData("Tore XD Tant", null)]
    [InlineData(null, null)]
    public async Task persists_user_with_guild_avatar_and_guild_nick_if_available(string? guildUsername, string? guildAvatar)
    {
        // Arrange
        var client = GetAnonymousClient();
        var discordUser = new DiscordUser("123", "Tore Tang", null);

        AddHttpInterceptor(new AccessTokenInterceptor());
        AddHttpInterceptor(new RefreshTokenInterceptor());
        AddHttpInterceptor(new LookupInterceptor(discordUser));
        AddHttpInterceptor(new GuildMemberInterceptor(discordUser, guildUsername, guildAvatar));

        //Act
        var response = await MakeRequest(client);
        var body
            = await response.Content.ReadAsJsonAsync<TokenResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().NotBeNull();

        var userDocuments = GetCommittedDocuments<UserManager.UserDocument>();
        var userDocument = userDocuments.Should().ContainSingle(doc => doc.Id == discordUser.Id).Subject;
        Assert.Multiple(() => userDocument.GuildId.Should().Be(GuildId),
            () => userDocument.Name.Should().Be(guildUsername ?? discordUser.Username),
            () => userDocument.Avatar.Should().Be(guildAvatar ?? discordUser.Avatar));
    }
    
    [Fact]
    public async Task persists_user_when_user_is_not_member_of_guild()
    {
        // Arrange
        var client = GetAnonymousClient();
        var discordUser = new DiscordUser("123", "Tore Tang", null);

        AddHttpInterceptor(new AccessTokenInterceptor());
        AddHttpInterceptor(new RefreshTokenInterceptor());
        AddHttpInterceptor(new LookupInterceptor(discordUser));
        AddHttpInterceptor(new GuildMemberInterceptor(discordUser, false));

        //Act
        var response = await MakeRequest(client);
        var body
            = await response.Content.ReadAsJsonAsync<TokenResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().NotBeNull();

        var userDocuments = GetCommittedDocuments<UserManager.UserDocument>();
        var userDocument = userDocuments.Should().ContainSingle(doc => doc.Id == discordUser.Id).Subject;
        Assert.Multiple(() => userDocument.GuildId.Should().Be(GuildId),
            () => userDocument.Name.Should().Be(discordUser.Username),
            () => userDocument.Avatar.Should().Be(discordUser.Avatar));
    }
}