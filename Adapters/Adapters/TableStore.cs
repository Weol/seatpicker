using System.Text.Json;
using Azure;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Seatpicker.SeatContext.Domain;
using Seatpicker.SeatContext.Domain.Layout.Ports;
using Seatpicker.UserContext.Application.UserToken.Ports;
using Seatpicker.UserContext.Domain;
using Seatpicker.UserContext.Domain.Registration.Ports;
using User = Seatpicker.UserContext.Domain.User;

namespace Seatpicker.Adapters.Adapters;

internal class TableStore : ILookupTables
{
    private readonly BlobClient tableClient;

    public TableStore(IAzureClientFactory<BlobServiceClient> blovServiceClientFactory, IOptions<Options> options)
    {
        var tableServiceClient = blovServiceClientFactory.CreateClient(options.Value.Endpoint);

        tableServiceClient.CreateBlobContainer();
        
        tableClient = tableServiceClient
            .GetTableClient(options.Value.TableName);
    }
    
    public async Task<IEnumerable<Table>> Get(Guid id)
    {
        try
        {
            var userEntity = await tableClient.GetEntityAsync<TableEntity>("Default", id);
            var entity = userEntity.Value;

            var roles = JsonSerializer.Deserialize<IEnumerable<Role>>(entity.Roles) ?? throw new NullReferenceException();

            return new Table(
                entity.RowKey,
                id
                entity.Nick,
                entity.Avatar,
                roles,
                entity.CreatedAt);
        }
        catch (RequestFailedException e) when (e.Status == 404)
        {
            return null;
        }
    }

    private class TableEntity : ITableEntity 
    {
        public string RowKey { get; set; }
        public string PartitionKey { get; set; };
        public string Name { get; set; } = null!;
        public string Avatar { get; set; } = null!;
        public string Roles { get; set; } = null!;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? Timestamp
        {
            get => DateTimeOffset.Now;
            set
            {
            }
        }

        public ETag ETag { get; set; }
    }
    
    public class Options
    {
        public string Endpoint { get; set; } = null!;
        public string TableName { get; set; } = null!;
    }
}

internal static class TableStoreExtensions
{
    internal static IServiceCollection AddTableStore(this IServiceCollection services, Action<TableStore.Options> configureAction)
    {
        services.Configure(configureAction);
        
        services.AddAzureClients(builder =>
        {
            var options = new UserStore.Options();
            configureAction(options);
            
            builder.AddTableServiceClient(options.Endpoint)
                .WithName(options.Endpoint);
        });

        services
            .AddSingletonPortMapping<ILookupTables, TableStore>();

        return services;
    }
}
