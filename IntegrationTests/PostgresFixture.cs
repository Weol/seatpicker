using Marten;
using Marten.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Seatpicker.Infrastructure.Adapters.Database;
using Shared;
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
        var postgres = new PostgreSqlBuilder().Build();

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