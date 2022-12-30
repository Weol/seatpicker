using System.Collections.Concurrent;
using Azure;
using Azure.Data.Tables;
using Azure.Data.Tables.Models;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Seatpicker.IntegrationTests.Host.Adapters;

public class InMemoryTableClient : TableClient
{
    private readonly ConcurrentDictionary<string, object> store = new();

    public override Task<Response<T>> GetEntityAsync<T>(string partitionKey, string rowKey,
        IEnumerable<string> select = null,
        CancellationToken cancellationToken = new())
    {
        if (!store.TryGetValue(partitionKey + rowKey, out var entity))
            throw new RequestFailedException(404, "Entity not found");
        
        return Task.FromResult(Response.FromValue((T) entity, null));
    }

    public override Task<Response> UpsertEntityAsync<T>(T entity, TableUpdateMode mode = TableUpdateMode.Merge,
        CancellationToken cancellationToken = new())
    {
        store[entity.PartitionKey + entity.RowKey] = entity;

        return Task.FromResult<Response>(null!);
    }
}
    
public class InMemoryTableServiceClient : TableServiceClient
{
    private readonly ConcurrentDictionary<string, InMemoryTableClient> tableClients = new();

    public override TableClient GetTableClient(string tableName)
    {
        if (!tableClients.TryGetValue(tableName, out var client)) 
            return tableClients[tableName] = new InMemoryTableClient();

        return client;
    }

    public override Response<TableItem> CreateTableIfNotExists(string tableName, CancellationToken cancellationToken = new CancellationToken())
    {
        return Substitute.For<Response<TableItem>>();
    }
}

public static class UserStoreFakerExtensions
{
    public static IServiceCollection AddUserStoreFaker(this IServiceCollection services)
    {
        var factory = Substitute.For<IAzureClientFactory<TableServiceClient>>();

        var tableClients = new ConcurrentDictionary<string, InMemoryTableServiceClient>();
        factory.CreateClient(Arg.Any<string>()).Returns(info =>
        {
            var clientName = info.Arg<string>();
            if (!tableClients.TryGetValue(clientName, out var client)) 
                return tableClients[clientName] = new InMemoryTableServiceClient();
            return client;
        });

        return services.AddSingleton(factory);
    }
}