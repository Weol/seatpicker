using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using Seatpicker.Infrastructure.Adapters.Database.GuildRoleMapping;
using Seatpicker.Infrastructure.Authentication;
using Seatpicker.Infrastructure.Entrypoints.Http.Authentication.Discord;
using Seatpicker.IntegrationTests.HttpInterceptor.Discord;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.Authentication.Discord;

// ReSharper disable once InconsistentNaming
public abstract class LoginAndRenewBase : IntegrationTestBase
{
    public LoginAndRenewBase(TestWebApplicationFactory factory,
        PostgresFixture databaseFixture,
        ITestOutputHelper testOutputHelper) : base(
        factory,
        databaseFixture,
        testOutputHelper)
    {
    }

    protected abstract Task<HttpResponseMessage> MakeRequest(HttpClient client);

    private static async Task<TestEndpoint.Response> TestAuthentication(HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await client.GetAsync("authentication/discord/test");
        var body = await response.Content.ReadAsJsonAsync<TestEndpoint.Response>();

        if (body is null) throw new NullReferenceException();

        return body;
    }

    [Fact]
    public async Task succeeds_and_jwt_is_returned()
    {
        // Arrange
        var guild = SetupGuild();
        var client = GetAnonymousClient(GuildId);
        var discordUser = Generator.GenerateDiscordUser();

        AddHttpInterceptor(new AccessTokenInterceptor());
        AddHttpInterceptor(new RefreshTokenInterceptor());
        AddHttpInterceptor(new LookupInterceptor(discordUser));
        AddHttpInterceptor(new GuildMemberInterceptor(discordUser));

        //Act
        var response = await MakeRequest(client);
        var body = await response.Content.ReadAsJsonAsync<TokenResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Should().NotBeNull();

        var testResponse = await TestAuthentication(client, body!.Token);
        testResponse.Should().NotBeNull();

        Assert.Multiple(
            () => testResponse.Id.Should().Be(discordUser.Id),
            () => testResponse.Name.Should().Be(discordUser.Username),
            () => testResponse.Roles.Should().Contain(Role.User.ToString()));
    }

    [Theory]
    [InlineData(new[] { Role.User })]
    [InlineData(new[] { Role.Admin })]
    [InlineData(new[] { Role.Operator })]
    [InlineData(new[] { Role.Admin, Role.Operator })]
    [InlineData(new Role[] { })]
    public async Task succeeds_and_jwt_has_roles_according_to_mapping(Role[] roles)
    {
        // Arrange
        var client = GetAnonymousClient(GuildId);
        var discordUser = Generator.GenerateDiscordUser();
        var guildRoleId = "999999";

        var roleMapping = new GuildRoleMapping(GuildId,
            roles.Select(role =>
                new GuildRoleMappingEntry(guildRoleId, role)).ToArray());
        await SetupDocuments(GuildId, roleMapping);

        AddHttpInterceptor(new AccessTokenInterceptor());
        AddHttpInterceptor(new RefreshTokenInterceptor());
        AddHttpInterceptor(new LookupInterceptor(discordUser, accessTokenInterceptor));
        AddHttpInterceptor(new GuildMemberInterceptor(discordUser, guildRoleId));

        //Act
        var response = await MakeRequest(client);
        var body = await response.Content.ReadAsJsonAsync<TokenResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().NotBeNull();

        var testResponse = await TestAuthentication(client, body!.Token);
        testResponse.Should().NotBeNull();

        var expectedRoles = roles.Append(Role.User).Distinct();
        Assert.Multiple(
            () => testResponse.Id.Should().Be(discordUser.Id),
            () => testResponse.Name.Should().Be(discordUser.Username),
            () => testResponse.Roles.Should().BeEquivalentTo(expectedRoles.Select(role => role.ToString())));
    }

    [Theory]
    [InlineData("Tore XD Tang", "1231289378")]
    [InlineData(null, "1231289378")]
    [InlineData("Tore XD Tant", null)]
    [InlineData(null, null)]
    public async Task returns_guild_nickname_and_avatar_when_available(string? guildUsername, string? guildAvatar)
    {
        // Arrange
        var client = GetAnonymousClient(GuildId);
        var discordUser = Generator.GenerateDiscordUser();
        var guildRoleId = "999999";

        AddHttpInterceptor(new AccessTokenInterceptor());
        AddHttpInterceptor(new RefreshTokenInterceptor());
        AddHttpInterceptor(new LookupInterceptor(discordUser));
        AddHttpInterceptor(new GuildMemberInterceptor(discordUser, guildUsername, guildAvatar, guildRoleId));

        //Act
        var response = await MakeRequest(client);
        var body
            = await response.Content.ReadAsJsonAsync<TokenResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().NotBeNull();

        Assert.Multiple(
            () => body!.UserId.Should().Be(discordUser.Id),
            () => body!.Nick.Should().Be(guildUsername ?? discordUser.Username),
            () => body!.Avatar.Should().Be(guildAvatar ?? discordUser.Avatar));
    }

    [Fact]
    public async Task succeeds_when_user_is_not_member_of_guild()
    {
        // Arrange
        var client = GetAnonymousClient(GuildId);
        var discordUser = Generator.GenerateDiscordUser();

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

        Assert.Multiple(
            () => body!.UserId.Should().Be(discordUser.Id),
            () => body!.Nick.Should().Be(discordUser.Username),
            () => body!.Avatar.Should().Be(discordUser.Avatar));
    }

    [Fact]
    public async Task succeeds_when_user_has_no_avatar()
    {
        // Arrange
        var client = GetAnonymousClient(GuildId);
        var discordUser = Generator.GenerateDiscordUser();

        AddHttpInterceptor(new RefreshTokenInterceptor());
        AddHttpInterceptor(new RefreshTokenInterceptor());
        AddHttpInterceptor(new LookupInterceptor(discordUser));
        AddHttpInterceptor(new GuildMemberInterceptor(discordUser));

        //Act
        var response = await MakeRequest(client);
        var body
            = await response.Content.ReadAsJsonAsync<TokenResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().NotBeNull();

        Assert.Multiple(
            () => body!.UserId.Should().Be(discordUser.Id),
            () => body!.Nick.Should().Be(discordUser.Username),
            () => body!.Avatar.Should().BeNull());
    }
}