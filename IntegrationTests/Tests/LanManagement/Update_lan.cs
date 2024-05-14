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
public class Update_lan : IntegrationTestBase
{
    public Update_lan(
        TestWebApplicationFactory factory,
        PostgresFixture databaseFixture,
        ITestOutputHelper testOutputHelper) : base(factory, databaseFixture, testOutputHelper)
    {
    }

    private async Task<HttpResponseMessage> MakeRequest(
        HttpClient client,
        string guildId,
        Guid lanId,
        UpdateLan.Request request) =>
        await client.PutAsJsonAsync($"guild/{guildId}/lan/{lanId}", request);

    public static TheoryData<UpdateLan.Request> ValidUpdateRequests()
    {
        return new TheoryData<UpdateLan.Request>
        {
            Generator.UpdateLanRequest() with { Active = true },
            Generator.UpdateLanRequest() with { Active = false },
        };
    }

    [Theory]
    [MemberData(nameof(ValidUpdateRequests))]
    public async Task succeeds_when_valid(UpdateLan.Request request)
    {
        // Arrange
        var guildId = await CreateGuild();
        var client = GetClient(guildId, Role.Admin);

        var existingLan = LanGenerator.Create(guildId, CreateUser(guildId), id: request.Id);
        await SetupAggregates(guildId, existingLan);

        // Act
        var response = await MakeRequest(client, guildId, existingLan.Id, request);

        // Assert
        var committedAggregates = GetCommittedDocuments<ProjectedLan>(guildId);

        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () =>
            {
                var lan = committedAggregates.Should().ContainSingle().Subject;
                Assert.Multiple(
                    () => lan.Id.Should().Be(request.Id),
                    () => lan.Title.Should().Be(request.Title),
                    () => lan.Active.Should().Be(request.Active),
                    () => lan.Background.Should().Equal(request.Background));
            });
    }

    public static TheoryData<UpdateLan.Request> InvalidUpdateRequests()
    {
        return new TheoryData<UpdateLan.Request>
        {
            Generator.UpdateLanRequest() with { Background = null! },
            Generator.UpdateLanRequest() with { Title = null! },
            Generator.UpdateLanRequest() with { Title = "" },
            Generator.UpdateLanRequest() with { Background = Array.Empty<byte>() },
            Generator.UpdateLanRequest() with { Background = new byte[] { 1, 2, 3, 4 } },
        };
    }

    [Theory]
    [MemberData(nameof(InvalidUpdateRequests))]
    public async Task fails_when_invalid(UpdateLan.Request request)
    {
        // Arrange
        var guildId = await CreateGuild();
        var client = GetClient(guildId, Role.Admin);

        var existingLan = LanGenerator.Create(guildId, CreateUser(guildId));
        await SetupAggregates(guildId, existingLan);

        // Act
        var response = await MakeRequest(client, guildId, existingLan.Id, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task fails_when_model_id_does_not_match_path_id()
    {
        // Arrange
        var guildId = await CreateGuild();
        var client = GetClient(guildId, Role.Admin);

        var existingLan = LanGenerator.Create(guildId, CreateUser(guildId));
        await SetupAggregates(guildId, existingLan);

        // Act
        var response = await MakeRequest(client, guildId, existingLan.Id, Generator.UpdateLanRequest());

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
        var response = await MakeRequest(client, guildId, Guid.NewGuid(), Generator.UpdateLanRequest());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}