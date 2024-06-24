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
public class Update_lan : IntegrationTestBase
{
    public Update_lan(
        TestWebApplicationFactory factory,
        PostgresFixture databaseFixture,
        ITestOutputHelper testOutputHelper) : base(factory, databaseFixture, testOutputHelper)
    {
    }

    private static async Task<HttpResponseMessage> MakeRequest(
        HttpClient client,
        string guildId,
        Guid lanId,
        UpdateLan.Request request) =>
        await client.PutAsJsonAsync($"guild/{guildId}/lan/{lanId}", request);

    public static TheoryData<UpdateLan.Request> ValidUpdateRequests()
    {
        return new TheoryData<UpdateLan.Request>
        {
            UpdateLanRequest() with { Active = true },
            UpdateLanRequest() with { Active = false },
        };
    }

    [Theory]
    [MemberData(nameof(ValidUpdateRequests))]
    public async Task succeeds_when_valid(UpdateLan.Request request)
    {
        // Arrange
        var guild = await CreateGuild();
        var client = GetClient(guild.Id, Role.Admin);

        var existingLan = RandomData.Aggregates.Lan(guild.Id, CreateUser(guild.Id), id: request.Id);
        await SetupAggregates(guild.Id, existingLan);

        // Act
        var response = await MakeRequest(client, guild.Id, existingLan.Id, request);

        // Assert
        var committedAggregates = GetCommittedDocuments<ProjectedLan>(guild.Id);

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
            UpdateLanRequest() with { Background = null! },
            UpdateLanRequest() with { Title = null! },
            UpdateLanRequest() with { Title = "" },
            UpdateLanRequest() with { Background = Array.Empty<byte>() },
            UpdateLanRequest() with { Background = new byte[] { 1, 2, 3, 4 } },
        };
    }

    [Theory]
    [MemberData(nameof(InvalidUpdateRequests))]
    public async Task fails_when_invalid(UpdateLan.Request request)
    {
        // Arrange
        var guild = await CreateGuild();
        var client = GetClient(guild.Id, Role.Admin);

        var existingLan = RandomData.Aggregates.Lan(guild.Id, CreateUser(guild.Id));
        await SetupAggregates(guild.Id, existingLan);

        // Act
        var response = await MakeRequest(client, guild.Id, existingLan.Id, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task fails_when_model_id_does_not_match_path_id()
    {
        // Arrange
        var guild = await CreateGuild();
        var client = GetClient(guild.Id, Role.Admin);

        var existingLan = RandomData.Aggregates.Lan(guild.Id, CreateUser(guild.Id));
        await SetupAggregates(guild.Id, existingLan);

        // Act
        var response = await MakeRequest(client, guild.Id, existingLan.Id, UpdateLanRequest());

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
        var response = await MakeRequest(client, guild.Id, Guid.NewGuid(), UpdateLanRequest());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    public static UpdateLan.Request UpdateLanRequest()
    {
        return new UpdateLan.Request(
            Guid.NewGuid(),
            false,
            RandomData.Faker.Hacker.Noun(),
            RandomData.Aggregates.LanBackground());
    }
}