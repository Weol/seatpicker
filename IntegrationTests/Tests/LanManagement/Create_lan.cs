using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Seatpicker.Application.Features.Lan;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Entrypoints.Http.Lan;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.LanManagement;

// ReSharper disable once InconsistentNaming
[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
public class Create_lan(
    TestWebApplicationFactory factory,
    PostgresFixture databaseFixture,
    ITestOutputHelper testOutputHelper) : IntegrationTestBase(factory, databaseFixture, testOutputHelper)
{
    private static async Task<HttpResponseMessage> MakeRequest(HttpClient client, string guildId, CreateLan.Request request) =>
        await client.PostAsJsonAsync($"guild/{guildId}/lan", request);

    private static CreateLan.Request CreateLanRequest(string guildId)
    {
        return new CreateLan.Request(
            RandomData.Faker.Hacker.Noun(),
            RandomData.Aggregates.LanBackground());
    }

    [Fact]
    public async Task succeeds_when_lan_is_valid()
    {
        // Arrange
		var guild = await CreateGuild();
        var client = GetClient(guild.Id, Role.Admin);

        var request = CreateLanRequest(guild.Id);

        // Act
        var response = await MakeRequest(client, guild.Id, request);

        // Assert
        var committedProjections = await GetCommittedDocuments<ProjectedLan>(guild.Id);

        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () =>
            {
                var lan = committedProjections.Should().ContainSingle().Subject;
                Assert.Multiple(
                    () => lan.Title.Should().Be(request.Title),
                    () => lan.Background.Should().Equal(request.Background));
            });
    }

    [Fact]
    public async Task fails_when_background_is_not_svg()
    {
        // Arrange
		var guild = await CreateGuild();
        var client = GetClient(guild.Id, Role.Admin);

        var request = CreateLanRequest(guild.Id) with { Background = [1, 2, 3, 4, 5, 6] };

        // Act
        var response = await MakeRequest(client, guild.Id, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task fails_when_logged_in_user_has_insufficent_roles()
    {
        // Arrange
		var guild = await CreateGuild();
        var client = GetClient(guild.Id);

        // Act
        var response = await MakeRequest(client, guild.Id, CreateLanRequest(guild.Id));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}