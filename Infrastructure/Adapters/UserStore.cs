using System.Text.Json;
using Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Seatpicker.Host.Adapters;

internal class UserStore : IStoreUser, ILookupUser
{
    private readonly TableClient tableClient;

    public UserStore(IAzureClientFactory<TableServiceClient> tableClientFactory, IOptions<Options> options)
    {
        var tableServiceClient = tableClientFactory.CreateClient(options.Value.Endpoint);

        tableServiceClient.CreateTableIfNotExists(options.Value.TableName);
        
        tableClient = tableServiceClient
            .GetTableClient(options.Value.TableName);
    }

    public async Task Store(User user)
    {
        var jsonRoles = JsonSerializer.Serialize(user.Roles);

        await tableClient.UpsertEntityAsync(new UserEntity
        {
            RowKey= user.Id,
            Nick = user.Nick,
            Avatar = user.Avatar,
            Roles = jsonRoles,
            CreatedAt = user.CreatedAt
        });
    }
    
    public async Task<User?> Lookup(string id)
    {
        try
        {
            var userEntity = await tableClient.GetEntityAsync<UserEntity>("Default", id);
            var entity = userEntity.Value;

            var roles = JsonSerializer.Deserialize<IEnumerable<Role>>(entity.Roles) ?? throw new NullReferenceException();

            return new User(
                entity.RowKey,
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

    internal class UserEntity : ITableEntity 
    {
        public string RowKey { get; set; }
        public string PartitionKey { get; set; } = "Default";
        public string Nick { get; set; } = null!;
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

internal static class UserStoreExtensions
{
    internal static IServiceCollection AddUserStore(this IServiceCollection services, Action<UserStore.Options> configureAction)
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
            .AddSingletonPortMapping<IStoreUser, UserStore>()
            .AddSingletonPortMapping<ILookupUser, UserStore>();

        return services;
    }
}
