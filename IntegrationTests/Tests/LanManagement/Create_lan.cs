using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Seatpicker.Application.Features.Lans;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Entrypoints.Http.Lan;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.LanManagement;

// ReSharper disable once InconsistentNaming
public class Create_lan : IntegrationTestBase
{
    public Create_lan(TestWebApplicationFactory factory, PostgresFixture databaseFixture, ITestOutputHelper testOutputHelper) : base(
        factory,
        databaseFixture,
        testOutputHelper)
    {
    }

    private async Task<HttpResponseMessage> MakeRequest(HttpClient client, string guildId, CreateLan.Request request) =>
        await client.PostAsJsonAsync($"guild/{guildId}/lan", request);

    [Fact]
    public async Task succeeds_when_lan_is_valid()
    {
        // Arrange
		var guildId = await CreateGuild();
        var client = GetClient(guildId, Role.Admin);

        var request = Generator.CreateLanRequest(guildId);

        // Act
        var response = await MakeRequest(client, guildId, request);

        // Assert
        var committedProjections = GetCommittedDocuments<ProjectedLan>(guildId);

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
		var guildId = await CreateGuild();
        var client = GetClient(guildId, Role.Admin);

        var request = Generator.CreateLanRequest(guildId) with { Background = new byte[] { 1, 2, 3, 4, 5, 6 } };

        // Act
        var response = await MakeRequest(client, guildId, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task fails_when_logged_in_user_has_insufficent_roles()
    {
        // Arrange
		var guildId = await CreateGuild();
        var client = GetClient(guildId);

        // Act
        var response = await MakeRequest(client, guildId, Generator.CreateLanRequest(guildId));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}