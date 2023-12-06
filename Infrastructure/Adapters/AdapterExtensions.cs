using Seatpicker.Infrastructure.Adapters.Database;
using Seatpicker.Infrastructure.Adapters.SignalR;
using Seatpicker.Infrastructure.Authentication;

namespace Seatpicker.Infrastructure.Adapters;

public static class AdapterExtensions
{
    public static IServiceCollection AddAdapters(this IServiceCollection services)
    {
        return services
            .AddDatabase(ConfigureDatabase)
            .AddSignalRAdapter()
            .AddUserManager();
    }

    public static WebApplication UseAdapters(this WebApplication app)
    {
        app.UseSignalRAdapter();
        
        return app;
    }
    
    private static void ConfigureDatabase(DatabaseOptions options, IConfiguration configuration)
    {
        configuration.GetSection("Database").Bind(options);

        // Values from keyvault   
        options.Password = configuration["DatabaseAdminPassword"] ?? throw new NullReferenceException();
        options.User = configuration["DatabaseAdminUsername"] ?? throw new NullReferenceException();
    }
}