using Marten;
using Testcontainers.PostgreSql;
using Weasel.Core;

var container = new PostgreSqlBuilder().Build();

try
{
    await container.StartAsync();

    using var store = DocumentStore.For(
        options =>
        {
            options.ApplicationAssembly = typeof(Seatpicker.Infrastructure.Program).Assembly;
            options.Connection(container.GetConnectionString);
        });

    await store.Storage.ApplyAllConfiguredChangesToDatabaseAsync(AutoCreate.All);

    var connectionString = container.GetConnectionString();

    Environment.SetEnvironmentVariable("POSTGRES_CONNECTION_STRING", connectionString, EnvironmentVariableTarget.User);

    await Task.Delay(TimeSpan.FromDays(1));
}
finally
{
    Environment.SetEnvironmentVariable("POSTGRES_CONNECTION_STRING", null, EnvironmentVariableTarget.User);
}