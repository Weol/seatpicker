using System.Net;
using FluentAssertions;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Entrypoints.Http.Lan;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.LanManagement;

// ReSharper disable once InconsistentNaming
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

    private async Task<HttpResponseMessage> MakeRequest(HttpClient client) =>
        await client.GetAsync("lan");

    [Fact]
    public async Task returns_all_lans_that_exist_for_tenant()
    {
        // Arrange
		var guildId = CreateGuild();
        var client = GetClient(guildId);

        var existingLan = new[] { LanGenerator.Create(guildId), LanGenerator.Create(guildId) };
        await SetupAggregates(guildId, existingLan[0], existingLan[1]);

        //Act
        var response = await MakeRequest(client);
        var body = await response.Content.ReadAsJsonAsync<GetLan.Response[]>();

        //Assert
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
		var guildId = CreateGuild();
        var client = GetClient(guildId);

        //Act
        var response = await MakeRequest(client);
        var body = await response.Content.ReadAsJsonAsync<GetLan.Response[]>();

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().NotBeNull();
        body.Should().BeEmpty();
    }

    [Fact]
    public async Task returns_only_lan_that_belong_to_correct_tenant()
    {
        // Arrange
		var guildId = CreateGuild();
        foreach (var tenant in new[] { "123", "321" })
        {
            var lans = new[] { LanGenerator.Create(tenant), LanGenerator.Create(tenant) };
            await SetupAggregates(tenant, lans[0], lans[1]);

            //Act
            var client = GetClient(tenant, Role.Admin);
            var response = await MakeRequest(client);
            var body = await response.Content.ReadAsJsonAsync<GetLan.Response[]>();

            //Assert
            body.Should().NotBeNull();
            body.Should().HaveCount(2);

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