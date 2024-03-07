using System.Net.Http.Headers;
using Bogus;
using Marten;
using Marten.Storage;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        testOutputHelper.WriteLine(
            $"Using Postgres fixture with connection string: {databaseFixture.Container.GetConnectionString()}");

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
        var store = factory.Services.GetRequiredService<IDocumentStore>();

        var tenant = documents is IGlobalDocument[] ? Tenancy.DefaultTenantId : guildId;

        using var session = store.LightweightSession(tenant);
        foreach (var document in documents)
        {
            session.Store(document);
        }

        session.SaveChanges();

        return Task.CompletedTask;
    }

    protected async Task ClearDocumentsByType<TDocument>()
        where TDocument : IDocument
    {
        var store = factory.Services.GetRequiredService<IDocumentStore>();

        await store.Advanced.Clean.DeleteDocumentsByTypeAsync(typeof(TDocument));
    }

    protected IEnumerable<TDocument> GetCommittedDocuments<TDocument>(string? guildId = null)
        where TDocument : IDocument
    {
        guildId ??= Tenancy.DefaultTenantId;

        var repository = factory.Services.GetRequiredService<IDocumentRepository>();
        using var reader = repository.CreateReader(guildId);
        return reader.Query<TDocument>().AsEnumerable();
    }

    protected User CreateUser(string guildId)
    {
        return CreateIdentity(guildId).GetAwaiter().GetResult().User;
    }

    protected async Task<TestIdentity> CreateIdentity(string guildId, params Role[] roles)
    {
        if (roles.Length == 0) roles = new[] { Role.User };
        
        var authenticationService = factory.Services.GetRequiredService<AuthenticationService>();

        var (jwtToken, expiresAt, authenticationToken) = await authenticationService.Login(
            new Faker().Random.Int(99999).ToString(),
            AuthenticationProvider.Discord,
            new Faker().Name.FirstName(),
            new Faker().Random.Int(99999).ToString(),
            "asdasdasd",
            roles,
            guildId);

        var identity = new TestIdentity(
            new User(authenticationToken.Id,
                authenticationToken.Nick,
                authenticationToken.Avatar,
                authenticationToken.Roles),
            roles,
            jwtToken,
            guildId);

        return identity;
    }

    protected string CreateGuild()
    {
        return new Faker().Random.Int(1).ToString();
    }

    public record TestIdentity(User User, Role[] Roles, string Token, string GuildId);
}