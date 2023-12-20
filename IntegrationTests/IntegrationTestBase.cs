using System.Net.Http.Headers;
using Bogus;
using Marten;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Logging;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Seatpicker.Application.Features;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Adapters.Database;
using Seatpicker.Infrastructure.Authentication;
using Seatpicker.Infrastructure.Authentication.Discord;
using Seatpicker.IntegrationTests.HttpInterceptor;
using Shared;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssemblyFixture;

namespace Seatpicker.IntegrationTests;

public abstract class IntegrationTestBase : IClassFixture<PostgresFixture>, IClassFixture<TestWebApplicationFactory>
{
    private readonly WebApplicationFactory<Infrastructure.Program> factory;
    private readonly ITestOutputHelper testOutputHelper;

    public string GuildId { get; } = new Faker().Random.Int(1).ToString();

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

                            services.ConfigureMarten(options =>
                            {
                                options.MultiTenantedWithSingleServer(
                                    databaseFixture.Container.GetConnectionString());
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

        GetService<ITenantProvider>().GetTenant().Returns(GuildId);
    }

    protected TService GetService<TService>()
        where TService : notnull
    {
        return factory.Services.GetRequiredService<TService>();
    }

    protected HttpClient GetClient(TestIdentity identity)
    {
        var client = GetAnonymousClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", identity.Token);

        return client;
    }

    protected HttpClient GetClient(params Role[] roles)
    {
        var identity = CreateIdentity(roles)
            .GetAwaiter()
            .GetResult();

        return GetClient(identity);
    }

    protected HttpClient GetAnonymousClient()
    {
        var client = factory.CreateDefaultClient(factory.ClientOptions.BaseAddress,
            new HttpResponseLoggerHandler(testOutputHelper));
        client.DefaultRequestHeaders.Add("Seatpicker-Tenant", Guid.NewGuid().ToString());
        return client;
    }

    protected async Task SetupAggregates(params AggregateBase[] aggregates)
    {
        var repository = factory.Services.GetRequiredService<IAggregateRepository>();
        using var aggregateTransaction = repository.CreateTransaction();
        foreach (var aggregate in aggregates)
        {
            aggregateTransaction.Create(aggregate);
        }

        await aggregateTransaction.Commit();
    }

    protected async Task SetupDocuments<TDocument>(params TDocument[] documents)
        where TDocument : IDocument
    {
        var repository = factory.Services.GetRequiredService<IDocumentRepository>();
        using var documentTransaction = repository.CreateTransaction();
        foreach (var document in documents)
        {
            documentTransaction.Store(document);
        }

        await documentTransaction.Commit();
    }

    protected IEnumerable<TDocument> GetCommittedDocuments<TDocument>()
        where TDocument : IDocument
    {
        var repository = factory.Services.GetRequiredService<IDocumentRepository>();
        using var reader = repository.CreateReader();
        return reader.Query<TDocument>().AsEnumerable();
    }

    protected async Task<User> CreateUser()
    {
        var userDocument = new UserManager.UserDocument(
            Guid.NewGuid().ToString(),
            GuildId,
            new Faker().Name.FirstName(),
            null);

        await SetupDocuments(userDocument);

        return new User(userDocument.Id, userDocument.Name, userDocument.Avatar);
    }

    protected async Task<TestIdentity> CreateIdentity(params Role[] roles)
    {
        if (roles.Length == 0) roles = new[] { Role.User };

        var jwtTokenCreator = GetService<DiscordJwtTokenCreator>();

        var user = await CreateUser();

        var discordToken = new DiscordToken(
            Id: user.Id,
            Nick: user.Name,
            RefreshToken: "8ioq3",
            Avatar: user.Avatar,
            Roles: roles,
            GuildId: "123"
        );

        var (token, expiresAt) = await jwtTokenCreator.CreateToken(discordToken, roles);

        var identity = new TestIdentity(user, roles, token, GuildId);
        return identity;
    }

    protected internal void AddHttpInterceptor<TInterceptor>(TInterceptor interceptor)
        where TInterceptor : IInterceptor
    {
        GetService<InterceptingHttpMessageHandler>()
            .Interceptors.Add(interceptor);
    }

    public record TestIdentity(User User, Role[] Roles, string Token, string GuildId);
}