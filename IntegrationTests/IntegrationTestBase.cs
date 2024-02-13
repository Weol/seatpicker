using System.Net.Http.Headers;
using Bogus;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Seatpicker.Application.Features;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Adapters.Database;
using Seatpicker.Infrastructure.Authentication;
using Seatpicker.Infrastructure.Authentication.Discord;
using Seatpicker.Infrastructure.Entrypoints.Utils;
using Seatpicker.IntegrationTests.HttpInterceptor;
using Shared;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssemblyFixture;

namespace Seatpicker.IntegrationTests;

public abstract class IntegrationTestBase : IAssemblyFixture<PostgresFixture>, IClassFixture<TestWebApplicationFactory>
{
    private readonly WebApplicationFactory<Infrastructure.Program> factory;
    private readonly ITestOutputHelper testOutputHelper;

    protected string GuildId { get; } = new Faker().Random.Int(1).ToString();

    protected IntegrationTestBase(TestWebApplicationFactory factory,
        PostgresFixture databaseFixture,
        ITestOutputHelper testOutputHelper)
    {
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

        this.testOutputHelper = testOutputHelper;
    }

    protected HttpClient GetClient(string tenant, TestIdentity identity)
    {
        var client = GetAnonymousClient(tenant);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", identity.Token);

        return client;
    }

    protected HttpClient GetClient(string tenant, params Role[] roles)
    {
        var identity = CreateIdentity(tenant, roles)
            .GetAwaiter()
            .GetResult();

        return GetClient(tenant, identity);
    }

    protected HttpClient GetAnonymousClient(string tenant)
    {
        var client = factory.CreateDefaultClient(factory.ClientOptions.BaseAddress,
            new HttpResponseLoggerHandler(testOutputHelper));
        client.DefaultRequestHeaders.Add(TenantAuthorizationMiddleware.TenantHeaderName, tenant);
        return client;
    }

    protected async Task SetupAggregates(string tenant, params AggregateBase[] aggregates)
    {
        var repository = factory.Services.GetRequiredService<IAggregateRepository>();
        using var aggregateTransaction = repository.CreateTransaction(tenant);
        foreach (var aggregate in aggregates)
        {
            aggregateTransaction.Create(aggregate);
        }

        await aggregateTransaction.Commit();
    }

    protected async Task SetupDocuments<TDocument>(string tenant, params TDocument[] documents)
        where TDocument : IDocument
    {
        testOutputHelper.WriteLine("BEGIN SETUPDOCUMENTS: " + documents.Length);
        var repository = factory.Services.GetRequiredService<IDocumentRepository>();
        using var documentTransaction = repository.CreateTransaction(tenant);
        foreach (var document in documents)
        {
            documentTransaction.Store(document);
        }

        testOutputHelper.WriteLine("COMMITING...");
        await documentTransaction.Commit();
        testOutputHelper.WriteLine("END SETUPDOCUMENTS");
    }

    protected IEnumerable<TDocument> GetCommittedDocuments<TDocument>(string tenant)
        where TDocument : IDocument
    {
        var repository = factory.Services.GetRequiredService<IDocumentRepository>();
        using var reader = repository.CreateReader(tenant);
        return reader.Query<TDocument>().AsEnumerable();
    }

    protected async Task<User> CreateUser(string tenant)
    {
        var userDocument = new UserManager.UserDocument(
            Guid.NewGuid().ToString(),
            GuildId,
            new Faker().Name.FirstName(),
            null);

        await SetupDocuments(tenant, userDocument);

        return new User(userDocument.Id, userDocument.Name, userDocument.Avatar);
    }

    protected async Task<TestIdentity> CreateIdentity(string tenant, params Role[] roles)
    {
        if (roles.Length == 0) roles = new[] { Role.User };

        var jwtTokenCreator = factory.Services.GetRequiredService<DiscordJwtTokenCreator>();

        var user = await CreateUser(tenant);

        var discordToken = new DiscordToken(
            Id: user.Id,
            Nick: user.Name,
            RefreshToken: "8ioq3",
            Avatar: user.Avatar,
            Roles: roles,
            GuildId: tenant
        );

        var (token, _) = await jwtTokenCreator.CreateToken(discordToken, roles);

        var identity = new TestIdentity(user, roles, token, GuildId);
        return identity;
    }

    protected void AddHttpInterceptor<TInterceptor>(TInterceptor interceptor)
        where TInterceptor : IInterceptor
    {
        var interceptingHttpHandler = factory.Services.GetRequiredService<InterceptingHttpMessageHandler>();
        interceptingHttpHandler.Interceptors.Add(interceptor);
    }

    public record TestIdentity(User User, Role[] Roles, string Token, string GuildId);
}