using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Adapters.Guilds;
using Seatpicker.Infrastructure.Entrypoints.Http.Guild;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.Guild;

public class Update_guild : IntegrationTestBase
{
    public Update_guild(
        TestWebApplicationFactory factory,
        PostgresFixture databaseFixture,
        ITestOutputHelper testOutputHelper) : base(factory, databaseFixture, testOutputHelper)
    {
    }

    private async Task<HttpResponseMessage>
        MakeRequest(HttpClient client, string guildId, UpdateGuild.Request request) =>
        await client.PutAsJsonAsync($"guild/{guildId}", request);

    public static TheoryData<UpdateGuild.Request> ValidUpdateRequests()
    {
        return new TheoryData<UpdateGuild.Request>
        {
            Generator.UpdateGuildRequest(),
            Generator.UpdateGuildRequest() with { Icon = "12312321" },
            Generator.UpdateGuildRequest() with { Hostnames = Array.Empty<string>() },
            Generator.UpdateGuildRequest() with
            {
                RoleMapping = new[] { new UpdateGuild.RoleMapping("123", new[] { Role.Admin }) },
            },
        };
    }

    [Theory]
    [MemberData(nameof(ValidUpdateRequests))]
    public async Task succeeds_when_valid(UpdateGuild.Request request)
    {
        // Arrange
        var guildId = await CreateGuild(request.Id);
        var client = GetClient(guildId, Role.Admin);

        var defaultDocument = GetCommittedDocuments<GuildAdapter.GuildDocument>()
            .First(guild => guild.Id == request.Id);

        // Act
        var response = await MakeRequest(client, guildId, request);

        // Assert
        var commitedDocument = GetCommittedDocuments<GuildAdapter.GuildDocument>()
            .First(guild => guild.Id == request.Id);

        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () =>
            {
                Assert.Multiple(
                    () => commitedDocument.Id.Should().Be(defaultDocument.Id),
                    () => commitedDocument.Icon.Should().Be(defaultDocument.Icon),
                    () => commitedDocument.Name.Should().Be(defaultDocument.Name),
                    () => commitedDocument.Hostnames.Should().Equal(request.Hostnames),
                    () =>
                    {
                        if (commitedDocument.RoleMappings.Any())
                        {
                            commitedDocument.RoleMappings.Should()
                                .AllSatisfy(
                                    mapping =>
                                    {
                                        var subject = commitedDocument.RoleMappings.Should()
                                            .ContainSingle(x => x.RoleId == mapping.RoleId)
                                            .Subject;

                                        subject.Roles.Should().Equal(mapping.Roles);
                                    });
                        }
                    });
            });
    }

    public static TheoryData<UpdateGuild.Request> InvalidUpdateRequests()
    {
        return new TheoryData<UpdateGuild.Request>
        {
            Generator.UpdateGuildRequest() with { Name = "" },
            Generator.UpdateGuildRequest() with { Hostnames = new[] { "%R%%!#$(/)" } },
            Generator.UpdateGuildRequest() with { Id = "" },
        };
    }

    [Theory]
    [MemberData(nameof(InvalidUpdateRequests))]
    public async Task fails_when_invalid(UpdateGuild.Request request)
    {
        // Arrange
        var guildId = await CreateGuild();
        var client = GetClient(guildId, Role.Admin);

        // Act
        var response = await MakeRequest(client, guildId, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task fails_when_model_id_does_not_match_path_id()
    {
        // Arrange
        var guildId = await CreateGuild();
        var client = GetClient(guildId, Role.Admin);

        // Act
        var response = await MakeRequest(client, guildId, Generator.UpdateGuildRequest());

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
        var response = await MakeRequest(client, guildId, Generator.UpdateGuildRequest());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}