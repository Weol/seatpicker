using System.Net.Http.Headers;
using System.Text;
using Bogus;
using JasperFx.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Seatpicker.Application.Features;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Authentication;
using Seatpicker.IntegrationTests.TestAdapters;
using Shared;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests;

public abstract class IntegrationTestBase : IDisposable
{
    private readonly WebApplicationFactory<Infrastructure.Program> factory;

    protected IntegrationTestBase(TestWebApplicationFactory factory, ITestOutputHelper testOutputHelper)
    {
        this.factory = factory.WithTestLogging(testOutputHelper);
    }

    protected TService GetService<TService>()
        where TService : notnull
    {
        return factory.Services.GetRequiredService<TService>();
    }

    protected HttpClient GetClient(TestIdentity identity)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", identity.Token);

        return client;
    }

    protected Task<TestIdentity> CreateIdentity(params Role[] roles)
    {
        var identityGenerator = factory.Services.GetRequiredService<IdentityGenerator>();
        return identityGenerator.GenerateWithRoles(roles);
    }

    protected HttpClient GetAnonymousClient() => factory.CreateClient();

    protected void SetupAggregates(params AggregateBase[] aggregates)
    {
        var repository = factory.Services.GetRequiredService<TestAggregateRepository>();
        var transaction = repository.CreateTransaction();
        foreach (var aggregate in aggregates)
        {
            transaction.Create(aggregate);
        }
        transaction.Commit();
    }

    protected void SetupDocuments(params IDocument[] documents)
    {
        var repository = factory.Services.GetRequiredService<TestDocumentRepository>();
        var transaction = repository.CreateTransaction();
        foreach (var document in documents)
        {
            transaction.Store(document);
        }
        transaction.Commit();
    }

    protected IEnumerable<TAggregate> GetCommittedAggregates<TAggregate>()
        where TAggregate : AggregateBase
    {
        var repository = factory.Services.GetRequiredService<TestAggregateRepository>();
        return repository.CreateReader().Query<TAggregate>();
    }

    protected IEnumerable<TDocument> GetCommittedDocuments<TDocument>()
        where TDocument : IDocument
    {
        var repository = factory.Services.GetRequiredService<TestDocumentRepository>();
        return repository.CreateReader().Query<TDocument>();
    }

    protected User CreateUser()
    {
        var userManager = GetService<UserManager>();
        var user = new User(new UserId(Guid.NewGuid().ToString()), new Faker().Name.FirstName());
        userManager.Store(user).GetAwaiter().GetResult();
        return user;
    }

    protected internal void MockOutgoingHttpRequest(
        Predicate<HttpRequestMessage> matcher,
        Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        GetService<InterceptingHttpMessageHandler>()
            .Interceptors.Add(new InterceptingHttpMessageHandler.Interceptor(matcher, responder));
    }

    public void Dispose()
    {
        GetService<InterceptingHttpMessageHandler>().Interceptors.Clear();
        GetService<TestAggregateRepository>().Aggregates.Clear();
        factory.Dispose();
    }

}