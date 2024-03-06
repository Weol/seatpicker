using System.Net;
using Bogus;
using FluentAssertions;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Authentication;
using Xunit;
using Xunit.Abstractions;
using User = Seatpicker.Infrastructure.Entrypoints.Http.User;

namespace Seatpicker.IntegrationTests.Tests.Guild;

// ReSharper disable once InconsistentNaming
public class Users : IntegrationTestBase
{
    public Users(TestWebApplicationFactory factory,
        PostgresFixture databaseFixture,
        ITestOutputHelper testOutputHelper) : base(
        factory,
        databaseFixture,
        testOutputHelper)
    {
    }

    [Fact]
    public async Task getting_users_returns_all_users_who_have_logged_in()
    {
        // Arrange
		var guildId = CreateGuild();
        var client = GetClient(guildId, Role.Operator);

        var users = Enumerable.Range(0, 5)
            .Select(i => new UserManager.UserDocument(i.ToString(),
                new Faker().Name.FirstName(),
                new Faker().Random.Int(1).ToString(),
                Array.Empty<Role>()))
            .ToArray();

        await SetupDocuments(guildId, users);

        //Act
        var response = await client.GetAsync($"guild/{guildId}/users");
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
            (Id: "123", Lans: new List<UserManager.UserDocument>()),
            (Id: "321", Lans: new List<UserManager.UserDocument>()),
            (Id: "111", Lans: new List<UserManager.UserDocument>()),
        };
        
        foreach (var (id, users) in guilds)
        {
            var generatedUsers = Enumerable.Range(0, 5)
                .Select(i => new UserManager.UserDocument(i.ToString(),
                    new Faker().Name.FirstName(),
                    new Faker().Random.Int(1).ToString(),
                    Array.Empty<Role>()))
                .ToArray();

            await SetupDocuments(id, generatedUsers);
            
            users.AddRange(generatedUsers);
        }

        foreach (var (guildId, users) in guilds)
        {
            var client = GetClient(guildId, Role.Operator);

            //Act
            var response = await client.GetAsync($"guild/{guildId}/users");
            var body = await response.Content.ReadAsJsonAsync<User[]>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            body.Should().NotBeNull();
            body.Should().HaveCount(users.Count);
            
            foreach (var user in users)
            {
                var retrievedUser = body.Should().ContainSingle(x => x.Id == user.Id).Subject;
                Assert.Multiple(
                    () => retrievedUser.Avatar.Should().Be(user.Avatar),
                    () => retrievedUser.Name.Should().Be(user.Name));
            }
        }
    }
}