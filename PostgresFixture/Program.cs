using Testcontainers.PostgreSql;

var container = new PostgreSqlBuilder()
    .Build();

const string environmentVariableName = "POSTGRES_CONNECTION_STRING";
try
{
    await container.StartAsync();

    var connectionString = container.GetConnectionString();

    Environment.SetEnvironmentVariable(environmentVariableName, connectionString, EnvironmentVariableTarget.User);
    Console.WriteLine("Setting environment variable " +  environmentVariableName + ": " + connectionString);
        
    await Task.Delay(TimeSpan.FromDays(1));
}
finally
{
    Environment.SetEnvironmentVariable(environmentVariableName, null, EnvironmentVariableTarget.User);
}