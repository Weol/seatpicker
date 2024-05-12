using DotNet.Testcontainers.Builders;
using Marten;
using Seatpicker.Infrastructure.Adapters.Database;
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
            .WithImage("postgres:16.2")
            .WithReuse(false)
            .WithAutoRemove(true)
            .WithCleanUp(true)
            .Build();

        await postgres.StartAsync();

        using var store = DocumentStore.For(
            options =>
            {
                DatabaseExtensions.ConfigureMarten(options, postgres.GetConnectionString());
            });

        await store.Storage.ApplyAllConfiguredChangesToDatabaseAsync(AutoCreate.All);

        container = postgres;
    }

    public async Task DisposeAsync()
    {
        if (container != null) await container.DisposeAsync();
    }
}