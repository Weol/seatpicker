using System.Net.Http.Headers;
using Marten.Storage;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Seatpicker.Application.Features.Lan;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Adapters.Database;
using Seatpicker.Infrastructure.Authentication;
using Seatpicker.IntegrationTests.TestAdapters;
using Shared;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssemblyFixture;

namespace Seatpicker.IntegrationTests;

public abstract class IntegrationTestBase : IAssemblyFixture<PostgresFixture>,
    IClassFixture<TestWebApplicationFactory>
{
    private readonly WebApplicationFactory<Infrastructure.Program> factory;
    private readonly ITestOutputHelper testOutputHelper;

    protected IntegrationTestBase(
        TestWebApplicationFactory factory,
        PostgresFixture databaseFixture,
        ITestOutputHelper testOutputHelper)
    {
        testOutputHelper.WriteLine(
            $"Using Postgres fixture with connection string: {databaseFixture.Container.GetConnectionString()}");

        this.testOutputHelper = testOutputHelper;
        this.factory = factory.WithWebHostBuilder(
            builder =>
            {
                builder.ConfigureServices(
                    services =>
                    {
                        services.PostConfigure<DatabaseOptions>(
                            options => { options.ConnectionString = databaseFixture.Container.GetConnectionString(); });

                        services.AddLogging(
                            loggingBuilder =>
                            {
                                loggingBuilder.ClearProviders();
                                loggingBuilder.AddDebug();
                                loggingBuilder.AddProvider(new XUnitLoggerProvider(testOutputHelper));
                            });
                    });
            });
    }

    protected T GetService<T>()
        where T : notnull
    {
        return factory.Services.GetRequiredService<T>();
    }

    protected HttpClient GetClient(TestIdentity identity)
    {
        var client = GetAnonymousClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", identity.Token);

        return client;
    }

    protected HttpClient GetClient(string guildId, params Role[] roles)
    {
        var identity = CreateIdentity(guildId, roles).GetAwaiter().GetResult();

        return GetClient(identity);
    }

    protected HttpClient GetAnonymousClient()
    {
        var client = factory.CreateDefaultClient(
            factory.ClientOptions.BaseAddress,
            new HttpResponseLoggerHandler(testOutputHelper));
        return client;
    }

    protected async Task SetupAggregates(string guildId, params AggregateBase[] aggregates)
    {
        var repository = GetService<AggregateRepository>();
        await using var aggregateTransaction = repository.CreateTransaction(guildId);
        foreach (var aggregate in aggregates)
        {
            aggregateTransaction.Create(aggregate);
        }

        await aggregateTransaction.Commit();
    }

    protected async Task SetupDocuments<TDocument>(params TDocument[] documents)
        where TDocument : IDocument
    {
        var repository = GetService<DocumentRepository>();
        var transaction = repository.CreateGuildlessTransaction();
        transaction.Store(documents);
        await transaction.Commit();
    }

    protected async Task SetupDocuments<TDocument>(string guildId, params TDocument[] documents)
        where TDocument : IDocument
    {
        var repository = GetService<DocumentRepository>();
        await using var transaction = repository.CreateTransaction(guildId);
        transaction.Store(documents);
        await transaction.Commit();
    }

    protected async Task<TDocument[]> GetCommittedDocuments<TDocument>()
        where TDocument : IDocument
    {
        var repository = factory.Services.GetRequiredService<DocumentRepository>();
        await using var reader = repository.CreateGuildlessReader();
        return reader.Query<TDocument>().ToArray();
    }

    protected async Task<TDocument[]> GetCommittedDocuments<TDocument>(string guildId)
        where TDocument : IDocument
    {
        var repository = GetService<DocumentRepository>();
        await using var reader = repository.CreateReader(guildId);
        return reader.Query<TDocument>().ToArray();
    }

    protected User CreateUser(string guildId)
    {
        return CreateIdentity(guildId).GetAwaiter().GetResult().User;
    }

    protected Task<TestIdentity> CreateIdentity(string guildId, params Role[] roles)
    {
        return CreateIdentity(guildId, roles, false);
    }

    protected async Task<TestIdentity> CreateIdentity(string guildId, Role[] roles, bool withoutGuildIdClaim)
    {
        if (roles.Length == 0) roles = [Role.User];

        var jwtTokenCreator = factory.Services.GetRequiredService<JwtTokenCreator>();

        var user = RandomData.User();
        var token = new AuthenticationToken(
            user.Id,
            user.Name,
            user.Avatar,
            "asdasdasd",
            roles,
            withoutGuildIdClaim ? null : guildId);

        var userDocument = new UserDocument(token.Id, token.Nick, token.Avatar, token.Roles);
        await SetupDocuments(guildId, userDocument);

        var (jwtToken, _) = await jwtTokenCreator.CreateToken(token);

        var identity = new TestIdentity(
            new User(userDocument.Id, userDocument.Name, userDocument.Avatar, userDocument.Roles),
            roles,
            jwtToken);

        return identity;
    }

    protected async Task<Guild> CreateGuild()
    {
        return await CreateGuild(RandomData.Guild());
    }

    protected async Task<Guild> CreateGuild(Guild guild)
    {
        var discordGuild = new Infrastructure.Adapters.Discord.DiscordGuild(
            guild.Id,
            guild.Name,
            guild.Icon,
            [RandomData.DiscordGuildRole()]);

        var discordAdapter = GetService<TestDiscordAdapter>();
        discordAdapter.AddGuild(
            discordGuild,
            guild.RoleMapping.Select(
                roleMapping => new Infrastructure.Adapters.Discord.DiscordGuildRole(
                    roleMapping.RoleId,
                    RandomData.Faker.Random.Word(),
                    RandomData.Faker.Random.Int(0, 16777215),
                    null)));

        await SetupDocuments(guild);

        return guild;
    }

    public record TestIdentity(User User, Role[] Roles, string Token);
}