using System.Diagnostics.CodeAnalysis;
using System.Net;
using FluentAssertions;
using Seatpicker.Domain;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.Guild;

// ReSharper disable once InconsistentNaming
[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
public class Users(
    TestWebApplicationFactory factory,
    PostgresFixture databaseFixture,
    ITestOutputHelper testOutputHelper) : IntegrationTestBase(factory, databaseFixture, testOutputHelper)
{
    [Fact]
    public async Task getting_users_returns_all_users_who_have_logged_in()
    {
        // Arrange
        var guild = await CreateGuild();
        var client = GetClient(guild.Id, Role.Operator);

        var users = new[]
        {
            CreateUser(guild.Id),
            CreateUser(guild.Id),
            CreateUser(guild.Id),
            CreateUser(guild.Id),
        };

        // Act
        var response = await client.GetAsync($"guild/{guild.Id}/users");
        var body = await response.Content.ReadAsJsonAsync<User[]>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().NotBeNull();

        foreach (var user in users)
        {
            var retrievedUser = body.Should().ContainSingle(x => x.Id == user.Id).Subject;
            Assert.Multiple(
                () => retrievedUser.Avatar.Should().Be(user.Avatar),
                () => retrievedUser.Name.Should().Be(user.Name));
        }
    }

    [Fact]
    public async Task getting_users_returns_only_users_from_single_guild()
    {
        // Arrange
        var guilds = new[]
        {
            (Guild: await CreateGuild(), Lans: new List<User>()),
            (Guild: await CreateGuild(), Lans: new List<User>()),
            (Guild: await CreateGuild(), Lans: new List<User>()),
        };

        foreach (var (guild, users) in guilds)
        {
            users.Add(CreateUser(guild.Id));
            users.Add(CreateUser(guild.Id));
            users.Add(CreateUser(guild.Id));
            users.Add(CreateUser(guild.Id));
        }

        foreach (var (guild, users) in guilds)
        {
            var identity = await CreateIdentity(guild.Id, Role.Operator);
            var client = GetClient(identity);

            // Act
            var response = await client.GetAsync($"guild/{guild.Id}/users");
            var body = await response.Content.ReadAsJsonAsync<User[]>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            body.Should().NotBeNull();
            body.Should().HaveCount(users.Count + 1); // + 1 because of the user created to perform the request

            foreach (var user in users)
            {
                var retrievedUser = body.Should().ContainSingle(x => x.Id == user.Id).Subject;
                Assert.Multiple(
                    () => retrievedUser.Avatar.Should().Be(user.Avatar),
                    () => retrievedUser.Name.Should().Be(user.Name));
            }

            {
                var retrievedUser = body.Should().ContainSingle(x => x.Id == identity.User.Id).Subject;
                Assert.Multiple(
                    () => retrievedUser.Avatar.Should().Be(identity.User.Avatar),
                    () => retrievedUser.Name.Should().Be(identity.User.Name));
            }
        }
    }
}