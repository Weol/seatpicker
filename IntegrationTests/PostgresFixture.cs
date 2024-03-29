using Marten;
using Testcontainers.PostgreSql;
using Weasel.Core;
using Xunit;
using Xunit.Extensions.AssemblyFixture;

[assembly: TestFramework(AssemblyFixtureFramework.TypeName, AssemblyFixtureFramework.AssemblyName)]
namespace Seatpicker.IntegrationTests;

public class PostgresFixture : IAsyncLifetime
{
    private PostgreSqlContainer? container;

    public PostgreSqlContainer Container => container ?? throw new NullReferenceException(); 

    public async Task InitializeAsync()
    {
        var postgres = new PostgreSqlBuilder()
            .Build();

        await postgres.StartAsync();

        using var store = DocumentStore.For(options =>
        {
            options.ApplicationAssembly = typeof(Program).Assembly;
            options.Connection(postgres.GetConnectionString);
        });

        await store.Storage.ApplyAllConfiguredChangesToDatabaseAsync(AutoCreate.All);

        container = postgres;
    }

    public async Task DisposeAsync()
    {
        if (container != null) await container.DisposeAsync();
    }
}