using System.Net;
using FluentAssertions;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Entrypoints.Http.Lan;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.LanManagement;

// ReSharper disable once InconsistentNaming
public class Get_lan : IntegrationTestBase
{
    public Get_lan(TestWebApplicationFactory factory,
        PostgresFixture databaseFixture,
        ITestOutputHelper testOutputHelper) : base(
        factory,
        databaseFixture,
        testOutputHelper)
    {
    }

    private async Task<HttpResponseMessage> MakeRequest(HttpClient client, string guildId, Guid lanId) =>
        await client.GetAsync($"guild/{guildId}/lan/{lanId}");

    private async Task<HttpResponseMessage> MakeRequest(HttpClient client, string guildId) =>
        await client.GetAsync($"guild/{guildId}/lan");

    [Fact]
    public async Task returns_all_lans_in_guild()
    {
        // Arrange
        var guildId = await CreateGuild();
        var client = GetClient(guildId, Role.Admin);

        var existingLans = new[]
        {
            LanGenerator.Create(guildId, CreateUser(guildId)),
            LanGenerator.Create(guildId, CreateUser(guildId)),
            LanGenerator.Create(guildId, CreateUser(guildId)),
        };
        await SetupAggregates(guildId, existingLans[0], existingLans[1], existingLans[2]);

        //Act
        var response = await MakeRequest(client, guildId);
        var body = await response.Content.ReadAsJsonAsync<LanResponse[]>();

        //Assert
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
        var guildId = await CreateGuild();
        var client = GetClient(guildId, Role.Admin);

        var existingLan = LanGenerator.Create(guildId, CreateUser(guildId));
        await SetupAggregates(guildId, existingLan);

        //Act
        var response = await MakeRequest(client, guildId, existingLan.Id);
        var body = await response.Content.ReadAsJsonAsync<LanResponse>();

        //Assert
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
        var guildId = await CreateGuild();
        var client = GetClient(guildId, Role.Admin);

        //Act
        var response = await MakeRequest(client, guildId, Guid.NewGuid());

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}