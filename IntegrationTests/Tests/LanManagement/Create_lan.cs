using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Seatpicker.Application.Features.Lans;
using Seatpicker.Infrastructure.Authentication;
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

    private async Task<HttpResponseMessage> MakeRequest(HttpClient client, CreateEndpoint.Request request) =>
        await client.PostAsJsonAsync("lan", request);

    [Fact]
    public async Task succeeds_when_lan_is_valid()
    {
        // Arrange
        var client = GetClient(GuildId, Role.Admin);

        var request = Generator.CreateLanRequest(GuildId);

        //Act
        var response = await MakeRequest(client, request);

        //Assert
        var committedProjections = GetCommittedDocuments<ProjectedLan>(GuildId);

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
        var client = GetClient(GuildId, Role.Admin);

        var request = Generator.CreateLanRequest(GuildId) with { Background = new byte[] { 1, 2, 3, 4, 5, 6 } };

        //Act
        var response = await MakeRequest(client, request);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task fails_when_logged_in_user_has_insufficent_roles()
    {
        // Arrange
        var client = GetClient(GuildId);

        //Act
        var response = await MakeRequest(client, Generator.CreateLanRequest(GuildId));

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}