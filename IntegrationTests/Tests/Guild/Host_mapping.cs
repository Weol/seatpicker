using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Adapters.Database.GuildHostMapping;
using Seatpicker.Infrastructure.Adapters.Database.GuildRoleMapping;
using Seatpicker.Infrastructure.Entrypoints.Http.Guild;
using Seatpicker.Infrastructure.Entrypoints.Http.Guild.Discord;
using Seatpicker.IntegrationTests.TestAdapters;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.Guild;

// ReSharper disable once InconsistentNaming
[Collection("GuildHostMapping")]
public class Host_mapping : IntegrationTestBase
{
    public Host_mapping(TestWebApplicationFactory factory,
        PostgresFixture databaseFixture,
        ITestOutputHelper testOutputHelper) : base(
        factory,
        databaseFixture,
        testOutputHelper)
    {
    }

    protected override void ConfigureServices(IServiceCollection services, PostgresFixture postgresFixture)
    {
        base.ConfigureServices(services, postgresFixture);
        services.PostConfigure()
    }

    [Fact]
    public async Task get_succeeds()
    {
        // Arrange
        var guild1 = CreateGuild();
        var guild2 = CreateGuild();
        var client = GetClient(guild2, Role.Superadmin);

        await ClearDocumentsByType<GuildHostMapping>();
        await SetupDocuments(guild2,
            new GuildHostMapping("guild1.host1", guild1),
            new GuildHostMapping("guild1.host2", guild1));
        await SetupDocuments(guild2, new GuildHostMapping("guild2.host1", guild2));

        //Act
        var response = await client.GetAsync("guild/hosts");
        var body = await response.Content.ReadAsJsonAsync<GetHostMapping.Response[]>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().HaveCount(2);

        Assert.Multiple(
            () =>
            {
                var mapping = body.Should().ContainSingle(x => x.GuildId == guild1).Subject;
                mapping.Hostnames.Should().HaveCount(2);
                mapping.Hostnames.Should().ContainSingle(x => x == "guild1.host1");
                mapping.Hostnames.Should().ContainSingle(x => x == "guild1.host2");
            },
            () =>
            {
                var mapping = body.Should().ContainSingle(x => x.GuildId == guild2).Subject;
                mapping.Hostnames.Should().HaveCount(1);
                mapping.Hostnames.Should().ContainSingle(x => x == "guild2.host1");
            });
    }

    [Fact]
    public async Task get_succeeds_when_there_are_no_host_mappings()
    {
        // Arrange
        var guildId = CreateGuild();
        var client = GetClient(guildId, Role.Superadmin);

        await ClearDocumentsByType<GuildHostMapping>();

        //Act
        var response = await client.GetAsync("guild/hosts");
        var body = await response.Content.ReadAsJsonAsync<GetHostMapping.Response[]>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().HaveCount(0);
    }

    [Fact]
    public async Task get_fails_when_user_is_not_superadmin()
    {
        // Arrange
        var guildId = CreateGuild();
        var client = GetClient(guildId, Role.Admin);

        //Act
        var response = await client.GetAsync("guild/hosts");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
    
    [Fact]
    public async Task put_fails_when_user_is_not_superadmin()
    {
        // Arrange
        var guildId = CreateGuild();
        var client = GetClient(guildId, Role.Admin);

        //Act
        var response = await client.PutAsJsonAsync("guild/hosts", Array.Empty<GuildHostMapping>());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task put_deletes_any_mappings_not_included_in_request()
    {
        // Arrange
        var guildId1 = CreateGuild();
        var guildId2 = CreateGuild();
        var client = GetClient(guildId1, Role.Superadmin);
        
        await ClearDocumentsByType<GuildHostMapping>();
        await SetupDocuments(guildId2,
            new GuildHostMapping("guild1.host1", guildId1),
            new GuildHostMapping("guild1.host2", guildId1));
        await SetupDocuments(guildId2, new GuildHostMapping("guild2.host1", guildId2));
        
        //Act
        var response = await client.PutAsJsonAsync("guild/hosts",
            new[]
            {
                new PutHostMapping.Request(guildId1,
                    new[]
                    {
                        "tore.tang",
                    })
            });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var commited = GetCommittedDocuments<GuildHostMapping>();

        Assert.Multiple(
            () => commited.Should().HaveCount(1),
            () =>
            {
                var mapping = commited.Should().ContainSingle(x => x.Hostname == "tore.tang").Subject;
                mapping.GuildId.Should().Be(guildId1);
            });
;
    }
    
    [Fact]
    public async Task put_fails_when_attempting_to_use_same_host_multiple_times()
    {
        // Arrange
        var guildId1 = CreateGuild();
        var guildId2 = CreateGuild();
        var client = GetClient(guildId1, Role.Superadmin);
        await ClearDocumentsByType<GuildHostMapping>();
        
        //Act
        var response = await client.PutAsJsonAsync("guild/hosts",
            new[]
            {
                new PutHostMapping.Request(guildId1,
                    new[]
                    {
                        "tore.tang",
                    }),
                new PutHostMapping.Request(guildId2,
                    new[]
                    {
                        "tore.tang",
                    })
            });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var commited = GetCommittedDocuments<GuildHostMapping>();

        commited.Should().HaveCount(0);
    }
    
    [Fact]
    public async Task put_succeeds()
    {
        // Arrange
        var guildId1 = CreateGuild();
        var guildId2 = CreateGuild();
        var client = GetClient(guildId1, Role.Superadmin);
        await ClearDocumentsByType<GuildHostMapping>();
        
        //Act
        var response = await client.PutAsJsonAsync("guild/hosts",
            new[]
            {
                new PutHostMapping.Request(guildId1,
                    new[]
                    {
                        "test.testerson",
                        "tore.tang",
                    }),
                new PutHostMapping.Request(guildId2,
                    new[]
                    {
                        "asd.asd",
                    })
            });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var commited = GetCommittedDocuments<GuildHostMapping>();

        Assert.Multiple(
            () => commited.Should().HaveCount(3),
            () =>
            {
                var mapping = commited.Should().ContainSingle(x => x.Hostname == "tore.tang").Subject;
                mapping.GuildId.Should().Be(guildId1);
            },
            () =>
            {
                var mapping = commited.Should().ContainSingle(x => x.Hostname == "test.testerson").Subject;
                mapping.GuildId.Should().Be(guildId1);
            },
            () =>
            {
                var mapping = commited.Should().ContainSingle(x => x.Hostname == "asd.asd").Subject;
                mapping.GuildId.Should().Be(guildId2);
            });
    }
}