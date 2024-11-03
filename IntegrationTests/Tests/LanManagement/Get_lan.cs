using System.Diagnostics.CodeAnalysis;
using System.Net;
using FluentAssertions;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Entrypoints.Http.Lan;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.LanManagement;

// ReSharper disable once InconsistentNaming
[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
public class Get_lan(
    TestWebApplicationFactory fusery,
    PostgresFixture databaseFixture,
    ITestOutputHelper testOutputHelper) : IntegrationTestBase(fusery, databaseFixture, testOutputHelper)
{
    private static async Task<HttpResponseMessage> MakeRequest(HttpClient client, string guildId, string lanId) =>
        await client.GetAsync($"guild/{guildId}/lan/{lanId}");

    private static async Task<HttpResponseMessage> MakeRequest(HttpClient client, string guildId) =>
        await client.GetAsync($"guild/{guildId}/lan");

    [Fact]
    public async Task returns_all_lans_in_guild()
    {
        // Arrange
        var guild = await CreateGuild();
        var client = GetClient(guild.Id, Role.Admin);

        var existingLans = new[]
        {
            RandomData.Aggregates.Lan(guild.Id, CreateUser(guild.Id)),
            RandomData.Aggregates.Lan(guild.Id, CreateUser(guild.Id)),
            RandomData.Aggregates.Lan(guild.Id, CreateUser(guild.Id)),
        };
        await SetupAggregates(guild.Id, existingLans[0], existingLans[1], existingLans[2]);

        // Act
        var response = await MakeRequest(client, guild.Id);
        var body = await response.Content.ReadAsJsonAsync<LanResponse[]>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().HaveCount(existingLans.Length);

        foreach (var existingLan in existingLans)
        {
            var lan = body.Should().ContainSingle(lan => lan.Id == existingLan.Id).Subject;

            Assert.Multiple(
                () => lan.Id.Should().Be(existingLan.Id),
                () => lan.Title.Should().Be(existingLan.Title),
                () => lan.Background.Should().Equal(existingLan.Background)
            );
        }
    }

    [Fact]
    public async Task returns_lan_when_lan_exists()
    {
        // Arrange
        var guild = await CreateGuild();
        var client = GetClient(guild.Id, Role.Admin);

        var existingLan = RandomData.Aggregates.Lan(guild.Id, CreateUser(guild.Id));
        await SetupAggregates(guild.Id, existingLan);

        // Act
        var response = await MakeRequest(client, guild.Id, existingLan.Id);
        var body = await response.Content.ReadAsJsonAsync<LanResponse>();

        // Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () =>
            {
                body.Should().NotBeNull();
                Assert.Multiple(
                    () => body!.Id.Should().Be(existingLan.Id),
                    () => body!.Title.Should().Be(existingLan.Title),
                    () => body!.Background.Should().Equal(existingLan.Background));
            });
    }

    [Fact]
    public async Task returns_nothing_when_lan_does_not_exist()
    {
        // Arrange
        var guild = await CreateGuild();
        var client = GetClient(guild.Id, Role.Admin);

        // Act
        var response = await MakeRequest(client, guild.Id, Guid.NewGuid());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}