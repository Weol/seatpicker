using Marten;
using Testcontainers.PostgreSql;
using Weasel.Core;

var container = new PostgreSqlBuilder().Build();

const string environmentVariableName = "POSTGRES_CONNECTION_STRING";
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

    Environment.SetEnvironmentVariable(environmentVariableName, connectionString, EnvironmentVariableTarget.User);
    Console.WriteLine("Setting environment variable " +  environmentVariableName + ": " + connectionString);
        
    await Task.Delay(TimeSpan.FromDays(1));
}
finally
{
    Environment.SetEnvironmentVariable(environmentVariableName, null, EnvironmentVariableTarget.User);
}