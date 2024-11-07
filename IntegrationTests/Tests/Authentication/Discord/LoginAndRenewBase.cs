using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using Seatpicker.Application.Features.Lan;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Authentication;
using Seatpicker.Infrastructure.Entrypoints.Http.Authentication;
using Seatpicker.IntegrationTests.TestAdapters;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.Authentication.Discord;

// ReSharper disable once InconsistentNaming
[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
public abstract class LoginAndRenewBase(
    TestWebApplicationFactory fusery,
    PostgresFixture databaseFixture,
    ITestOutputHelper testOutputHelper) : IntegrationTestBase(fusery, databaseFixture, testOutputHelper)
{
    protected string DiscordToken { get; set; } = null!;
    protected string RefreshToken { get; set; } = null!;

    protected abstract Task<HttpResponseMessage> MakeRequest(HttpClient client, string guildId);

    private static async Task<TestEndpoint.Response> TestAuthentication(HttpClient client, string guildId, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await client.GetAsync($"authentication/test/{guildId}");
        var body = await response.Content.ReadAsJsonAsync<TestEndpoint.Response>();

        if (body is null) throw new Exception("asd");

        return body;
    }

    [Fact]
    public async Task succeeds_when_member_of_guild_and_jwt_is_returned()
    {
        // Arrange
        var guild = await CreateGuild();
        var client = GetAnonymousClient();
        var discordUser = RandomData.DiscordUser();

        (DiscordToken, RefreshToken) = GetService<TestDiscordAdapter>().AddUser(discordUser, guild.Id);

        // Act
        var response = await MakeRequest(client, guild.Id);
        var body = await response.Content.ReadAsJsonAsync<TokenResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Should().NotBeNull();

        var testResponse = await TestAuthentication(client, guild.Id, body!.Token);
        testResponse.Should().NotBeNull();

        var userId = discordUser.Id;

        Assert.Multiple(
            () => testResponse.Id.Should().Be(userId),
            () => testResponse.Name.Should().Be(discordUser.Username),
            () => testResponse.Roles.Should().Contain(Role.User));
    }

    [Theory]
    [InlineData(new[] { Role.User })]
    [InlineData(new[] { Role.Admin })]
    [InlineData(new[] { Role.Operator })]
    [InlineData(new[] { Role.Admin, Role.Operator })]
    public async Task succeeds_and_jwt_has_roles_according_to_mapping(Role[] roles)
    {
        // Arrange
        var rolesWithUser = roles.Append(Role.User).Distinct().ToArray();

        var guildRole = RandomData.GuildRole();
        var guild = await CreateGuild(RandomData.Guild() with
        {
            RoleMapping = [new GuildRoleMapping(guildRole.Id, roles)],
            Roles = [guildRole]
        });
        var client = GetAnonymousClient();
        var discordUser = RandomData.DiscordUser();

        (DiscordToken, RefreshToken) = GetService<TestDiscordAdapter>()
            .AddUser(discordUser, guild.Id, null, null, guildRole.Id);

        // Act
        var response = await MakeRequest(client, guild.Id);
        var body = await response.Content.ReadAsJsonAsync<TokenResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().NotBeNull();

        var testResponse = await TestAuthentication(client, guild.Id, body!.Token);
        testResponse.Should().NotBeNull();

        var expectedRoles = rolesWithUser.Append(Role.User).Distinct();
        Assert.Multiple(
            () => testResponse.Id.Should().Be(discordUser.Id),
            () => testResponse.Name.Should().Be(discordUser.Username),
            () => testResponse.Roles.Should().BeEquivalentTo(expectedRoles));
    }

    [Theory]
    [InlineData("Tore XD Tang", "1231289378")]
    [InlineData(null, "1231289378")]
    [InlineData("Tore XD Tang", null)]
    [InlineData(null, null)]
    public async Task returns_guild_nickname_and_avatar_when_available(string? guildUsername, string? guildAvatar)
    {
        // Arrange
        var guild = await CreateGuild();
        var client = GetAnonymousClient();
        var discordUser = RandomData.DiscordUser();

        (DiscordToken, RefreshToken)
            = GetService<TestDiscordAdapter>().AddUser(discordUser, guild.Id, guildUsername, guildAvatar);

        // Act
        var response = await MakeRequest(client, guild.Id);
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
    public async Task succeeds_when_not_member_of_guild_and_jwt_is_returned()
    {
        // Arrange
        var guild = await CreateGuild();
        var client = GetAnonymousClient();
        var discordUser = RandomData.DiscordUser();

        (DiscordToken, RefreshToken) = GetService<TestDiscordAdapter>().AddUser(discordUser);

        // Act
        var response = await MakeRequest(client, guild.Id);
        var body
            = await response.Content.ReadAsJsonAsync<TokenResponse>();

        // Assert
        var testResponse = await TestAuthentication(client, guild.Id, body!.Token);
        testResponse.Should().NotBeNull();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().NotBeNull();

        Assert.Multiple(
            () => testResponse.Id.Should().Be(discordUser.Id),
            () => testResponse.Name.Should().Be(discordUser.Username),
            () => testResponse.Roles.Should().Contain(Role.User));
    }

    [Fact]
    public async Task succeeds_when_user_has_no_avatar()
    {
        // Arrange
        var guild = await CreateGuild();
        var client = GetAnonymousClient();
        var discordUser = RandomData.DiscordUser() with
        {
            Avatar = null,
        };

        (DiscordToken, RefreshToken) = GetService<TestDiscordAdapter>().AddUser(discordUser, guild.Id);

        // Act
        var response = await MakeRequest(client, guild.Id);
        var body = await response.Content.ReadAsJsonAsync<TokenResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().NotBeNull();

        Assert.Multiple(
            () => body!.UserId.Should().Be(discordUser.Id),
            () => body!.Nick.Should().Be(discordUser.Username),
            () => body!.Avatar.Should().BeNull());
    }

    [Theory]
    [InlineData("Tore XD Tang", "1231289378")]
    [InlineData(null, "1231289378")]
    [InlineData("Tore XD Tant", null)]
    [InlineData(null, null)]
    public async Task persists_user_with_guild_avatar_and_guild_nick_if_available(string? guildUsername,
        string? guildAvatar)
    {
        // Arrange
        var guild = await CreateGuild();
        var client = GetAnonymousClient();
        var discordUser = RandomData.DiscordUser();

        (DiscordToken, RefreshToken)
            = GetService<TestDiscordAdapter>().AddUser(discordUser, guild.Id, guildUsername, guildAvatar);

        // Act
        var response = await MakeRequest(client, guild.Id);
        var body = await response.Content.ReadAsJsonAsync<TokenResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().NotBeNull();

        var userDocuments = GetCommittedDocuments<UserManager.UserDocument>(guild.Id);
        var userDocument = userDocuments.Should().ContainSingle(doc => doc.Id == discordUser.Id).Subject;

        Assert.Multiple(() => userDocument.Name.Should().Be(guildUsername ?? discordUser.Username),
            () => userDocument.Avatar.Should().Be(guildAvatar ?? discordUser.Avatar));
    }

    [Fact]
    public async Task persists_user_when_user_is_not_member_of_guild()
    {
        // Arrange
        var guild = await CreateGuild();
        var client = GetAnonymousClient();
        var discordUser = RandomData.DiscordUser();

        (DiscordToken, RefreshToken) = GetService<TestDiscordAdapter>().AddUser(discordUser);

        // Act
        var response = await MakeRequest(client, guild.Id);
        var body = await response.Content.ReadAsJsonAsync<TokenResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().NotBeNull();

        var userDocuments = GetCommittedDocuments<UserManager.UserDocument>(guild.Id);
        var userDocument = userDocuments.Should().ContainSingle(doc => doc.Id == discordUser.Id).Subject;
        Assert.Multiple(() => userDocument.Name.Should().Be(discordUser.Username),
            () => userDocument.Avatar.Should().Be(discordUser.Avatar));
    }
}