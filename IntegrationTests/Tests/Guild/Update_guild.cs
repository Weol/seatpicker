using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Seatpicker.Application.Features.Lan;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Entrypoints.Http.Guild;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.Guild;

[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
public class Update_guild(
    TestWebApplicationFactory factory,
    PostgresFixture databaseFixture,
    ITestOutputHelper testOutputHelper) : IntegrationTestBase(factory, databaseFixture, testOutputHelper)
{
    private static async Task<HttpResponseMessage>
        MakeRequest(HttpClient client, string guildId, UpdateGuild.Request request) =>
        await client.PutAsJsonAsync($"guild/{guildId}", request);

    private static UpdateGuild.Request UpdateGuildRequest(Application.Features.Lan.Guild guild)
    {
        return new UpdateGuild.Request(guild.Id,
            guild.Name,
            guild.Id,
            guild.Hostnames,
            guild.RoleMapping,
            guild.Roles);
    }

    public static TheoryData<Func<Application.Features.Lan.Guild, UpdateGuild.Request>> ValidUpdateRequests()
    {
        var data = new TheoryData<Func<Application.Features.Lan.Guild, UpdateGuild.Request>>
        {
            guild => UpdateGuildRequest(guild),
            guild => UpdateGuildRequest(guild) with { Hostnames = [] },
            guild => UpdateGuildRequest(guild) with { RoleMapping = [] },
        };
        
        return data;
    }

    [Theory]
    [MemberData(nameof(ValidUpdateRequests))]
    public async Task succeeds_when_valid(Func<Application.Features.Lan.Guild, UpdateGuild.Request> createRequest)
    {
        // Arrange
        var guild = await CreateGuild();
        var client = GetClient(guild.Id, Role.Admin);
        var request = createRequest(guild);

        var defaultGuilds = await GetCommittedDocuments<Application.Features.Lan.Guild>(guild.Id);
        var defaultDocument = defaultGuilds
            .First(document => document.Id == guild.Id);

        // Act
        var response = await MakeRequest(client, guild.Id, request);

        // Assert
        var committedGuilds = await GetCommittedDocuments<Application.Features.Lan.Guild>(guild.Id);
        var committedDocument = committedGuilds
            .First(document => document.Id == guild.Id);

        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () =>
            {
                Assert.Multiple(
                    () => committedDocument.Id.Should().Be(defaultDocument.Id),
                    () => committedDocument.Hostnames.Should().BeEquivalentTo(request.Hostnames),
                    () =>
                    {
                        committedDocument.RoleMapping.Should()
                            .AllSatisfy(
                                mapping =>
                                {
                                    var subject = committedDocument.RoleMapping.Should()
                                        .ContainSingle(x => x.RoleId == mapping.RoleId)
                                        .Subject;

                                    subject.Roles.Should().Equal(mapping.Roles);
                                });
                    });
            });
    }

    public static TheoryData<Func<Application.Features.Lan.Guild, UpdateGuild.Request>> InvalidUpdateRequests()
    {
        var data = new TheoryData<Func<Application.Features.Lan.Guild, UpdateGuild.Request>>()
        {
            // Should not be able to have empty or whitespace strings
            guild => UpdateGuildRequest(guild) with { Hostnames = [RandomData.Hostname()], Id = "" },
            guild => UpdateGuildRequest(guild) with { Hostnames = [RandomData.Hostname()], Id = "   " },
            guild => UpdateGuildRequest(guild) with { Hostnames = [RandomData.Hostname()], Name = "" },
            guild => UpdateGuildRequest(guild) with { Hostnames = [RandomData.Hostname()], Name = " " },
            
            // Should not be able to change Id
            guild => UpdateGuildRequest(guild) with { Hostnames = [RandomData.Hostname()], Id = "123" },
            
            // Should not be able to change name
            guild => UpdateGuildRequest(guild) with { Hostnames = [RandomData.Hostname()], Name = "asdasd" },

            // Should not be able to have duplicate hostnames
            guild => UpdateGuildRequest(guild) with { Hostnames = ["test.org", "test.org"] },

            // Should not be able to add a new role
            guild => UpdateGuildRequest(guild) with { Hostnames = [RandomData.Hostname()], Roles = [RandomData.GuildRole()] },

            // Should not be able to alter any roles
            guild =>
                UpdateGuildRequest(guild) with
                {
                    Hostnames = [RandomData.Hostname()], 
                    Roles = guild.Roles.Select(role => role with { Name = "Hehe" }).ToArray()
                },

            // Should not be able to create role mapping on a role that does not exist
            guild =>
                UpdateGuildRequest(guild) with
                {
                    Hostnames = [RandomData.Hostname()], 
                    RoleMapping =
                    [
                        new GuildRoleMapping(
                            RandomData.NotAnyOf(guild.Roles.Select(role => role.Id), RandomData.NumericId),
                            [Role.Operator]),
                    ]
                },
        };

        return data;
    }

    [Theory]
    [MemberData(nameof(InvalidUpdateRequests))]
    public async Task fails_when_invalid(Func<Application.Features.Lan.Guild, UpdateGuild.Request> createRequest)
    {
        // Arrange
        var guild = await CreateGuild();
        var client = GetClient(guild.Id, Role.Admin);
        var request = createRequest(guild);

        // Act
        var response = await MakeRequest(client, guild.Id, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task fails_when_model_id_does_not_match_path_id()
    {
        // Arrange
        var guild = await CreateGuild();
        var client = GetClient(guild.Id, Role.Admin);

        // Act
        var response = await MakeRequest(client, guild.Id, UpdateGuildRequest(guild) with { Id = "123" });

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
        var response = await MakeRequest(client, guild.Id, UpdateGuildRequest(guild));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}