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
public class GetAll_lan : IntegrationTestBase
{
    public GetAll_lan(TestWebApplicationFactory factory,
        PostgresFixture databaseFixture,
        ITestOutputHelper testOutputHelper) : base(
        factory,
        databaseFixture,
        testOutputHelper)
    {
    }

    private static async Task<HttpResponseMessage> MakeRequest(HttpClient client, string guildId) =>
        await client.GetAsync($"guild/{guildId}/lan");

    [Fact]
    public async Task returns_all_lans_that_exist_for_tenant()
    {
        // Arrange
        var guild = await CreateGuild();
        var client = GetClient(guild.Id);

        var existingLan = new[] { RandomData.Aggregates.Lan(guild.Id, CreateUser(guild.Id)), RandomData.Aggregates.Lan(guild.Id, CreateUser(guild.Id)) };
        await SetupAggregates(guild.Id, existingLan[0], existingLan[1]);

        // Act
        var response = await MakeRequest(client, guild.Id);
        var body = await response.Content.ReadAsJsonAsync<LanResponse[]>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        body.Should().NotBeNull();
        body.Should().HaveCount(2);

        foreach (var lan in existingLan)
        {
            var responseLan = body.Should().ContainSingle(x => lan.Id == x.Id).Subject;

            Assert.Multiple(
                () => responseLan.Id.Should().Be(lan.Id),
                () => responseLan.Title.Should().Be(lan.Title),
                () => responseLan.Background.Should().Equal(lan.Background));
        }
    }

    [Fact]
    public async Task returns_empty_array_when_no_lans_exist()
    {
        // Arrange
        var guild = await CreateGuild();
        var client = GetClient(guild.Id);

        // Act
        var response = await MakeRequest(client, guild.Id);
        var body = await response.Content.ReadAsJsonAsync<LanResponse[]>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().NotBeNull();
        body.Should().BeEmpty();
    }

    [Fact]
    public async Task returns_only_lan_that_belong_to_correct_guild()
    {
        // Arrange
        var guilds = new[]
        {
            (Id: await CreateGuild(), Lans: new List<Lan>()),
            (Id: await CreateGuild(), Lans: new List<Lan>()),
            (Id: await CreateGuild(), Lans: new List<Lan>()),
        };

        foreach (var (guild, lans) in guilds)
        {
            var generatedLans = new[] { RandomData.Aggregates.Lan(guild.Id, CreateUser(guild.Id)), RandomData.Aggregates.Lan(guild.Id, CreateUser(guild.Id))};
            await SetupAggregates(guild.Id, generatedLans[0], generatedLans[1]);
            lans.AddRange(generatedLans);
        }

        foreach (var (guild, lans) in guilds)
        {
            // Act
            var client = GetClient(guild.Id, Role.Admin);
            var response = await MakeRequest(client, guild.Id);
            var body = await response.Content.ReadAsJsonAsync<LanResponse[]>();

            // Assert
            body.Should().NotBeNull();
            body.Should().HaveCount(lans.Count);

            foreach (var lan in lans)
            {
                var responseLan = body.Should().ContainSingle(x => lan.Id == x.Id).Subject;

                Assert.Multiple(
                    () => responseLan.Id.Should().Be(lan.Id),
                    () => responseLan.Title.Should().Be(lan.Title),
                    () => responseLan.Background.Should().Equal(lan.Background));
            }
        }
    }
}