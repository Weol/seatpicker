using System.Net.Http.Headers;
using Bogus;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Seatpicker.Application.Features;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Adapters.Database;
using Seatpicker.Infrastructure.Authentication;
using Shared;
using Xunit.Abstractions;
using Xunit.Extensions.AssemblyFixture;

namespace Seatpicker.IntegrationTests;

public abstract class IntegrationTestBase : IAssemblyFixture<PostgresFixture>,
    IAssemblyFixture<TestWebApplicationFactory>
{
    private readonly WebApplicationFactory<Infrastructure.Program> factory;
    private readonly ITestOutputHelper testOutputHelper;

    protected IntegrationTestBase(TestWebApplicationFactory factory,
        PostgresFixture databaseFixture,
        ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
        this.factory = factory
            .WithWebHostBuilder(
                builder =>
                {
                    builder.ConfigureServices(
                        services =>
                        {
                            services.PostConfigure<DatabaseOptions>(options =>
                            {
                                options.ConnectionString = databaseFixture.Container.GetConnectionString();
                            });

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

    protected T GetService<T>() where T : notnull
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
        var identity = CreateIdentity(guildId, roles)
            .GetAwaiter()
            .GetResult();

        return GetClient(identity);
    }

    protected HttpClient GetAnonymousClient()
    {
        var client = factory.CreateDefaultClient(factory.ClientOptions.BaseAddress,
            new HttpResponseLoggerHandler(testOutputHelper));
        return client;
    }

    protected async Task SetupAggregates(string guildId, params AggregateBase[] aggregates)
    {
        var repository = factory.Services.GetRequiredService<IAggregateRepository>();
        using var aggregateTransaction = repository.CreateTransaction(guildId);
        foreach (var aggregate in aggregates)
        {
            aggregateTransaction.Create(aggregate);
        }

        await aggregateTransaction.Commit();
    }

    protected Task SetupDocuments<TDocument>(string guildId, params TDocument[] documents)
        where TDocument : IDocument
    {
        var repository = factory.Services.GetRequiredService<IDocumentRepository>();
        using var documentTransaction = repository.CreateTransaction(guildId);
        foreach (var document in documents)
        {
            documentTransaction.Store(document);
        }

        // We cant allow more than one test to run commit at a time otherwise there is a deadlock on macOS
        lock (factory)
        {
            documentTransaction.Commit().GetAwaiter().GetResult();
        }

        return Task.CompletedTask;
    }

    protected IEnumerable<TDocument> GetCommittedDocuments<TDocument>(string tenant)
        where TDocument : IDocument
    {
        var repository = factory.Services.GetRequiredService<IDocumentRepository>();
        using var reader = repository.CreateReader(tenant);
        return reader.Query<TDocument>().AsEnumerable();
    }

    protected async Task<User> CreateUser(string guildId)
    {
        var userDocument = new UserManager.UserDocument(
            Guid.NewGuid().ToString(),
            guildId,
            new Faker().Name.FirstName(),
            Array.Empty<Role>());

        await SetupDocuments(guildId, userDocument);

        return new User(userDocument.Id, userDocument.Name, userDocument.Avatar, Array.Empty<Role>());
    }

    protected async Task<TestIdentity> CreateIdentity(string guildId, params Role[] roles)
    {
        if (roles.Length == 0) roles = new[] { Role.User };

        var jwtTokenCreator = factory.Services.GetRequiredService<JwtTokenCreator>();

        var user = await CreateUser(guildId);

        var discordToken = new DiscordToken(
            Id: user.Id,
            Nick: user.Name,
            RefreshToken: "8ioq3",
            Avatar: user.Avatar,
            Roles: roles,
            GuildId: guildId,
            Provider: AuthenticationProvider.Discord
        );

        var (token, _) = await jwtTokenCreator.CreateToken(discordToken, roles);

        var identity = new TestIdentity(user, roles, token, guildId);
        return identity;
    }

    protected string CreateGuild()
    {
        return new Faker().Random.Int(1).ToString();
    }

    public record TestIdentity(User User, Role[] Roles, string Token, string GuildId);
}