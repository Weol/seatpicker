using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Seatpicker.Adapters;
using Seatpicker.Application;
using Seatpicker.Application.Middleware;
using Seatpicker.Host;
using Seatpicker.UserContext;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(builder =>
    {
        builder.UseWhen<ModelValidationExceptionHandlerMiddleware>(context => context.IsHttpTrigger());
        builder.UseWhen<JsonExceptionHandlerMiddleware>(context => context.IsHttpTrigger());
    })
    .ConfigureServices((context, services) =>
    {
        var config = context.Configuration;

        services
            .AddAdapters(new AdapterConfiguration(config))
            .AddApplication()
            .AddUserContext(new UserContextConfiguration(config));
    })
    .Build();

host.Run();

namespace Seatpicker.Host
{
    internal class AdapterConfiguration : IAdapterConfiguration
    {
        public AdapterConfiguration(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        public Uri DiscordBaseUri => new(Configuration["DiscordBaseUri"].TrimEnd('/') + "/");
        public Uri DiscordRedirectUri => new(Configuration["DiscordRedirectUri"]);
        public string ClientId => Configuration["ClientId"];
        public string ClientSecret => Configuration["ClientSecret"];
        public Guid LanId => Guid.Parse(Configuration["LanId"]);
        public Uri KeyvaultUri => new(Configuration["KeyvaultUri"]);
        public string StorageEndpoint => Configuration["StorageEndpoint"];
    }

    internal class UserContextConfiguration : IUserContextConfiguration
    {
        public UserContextConfiguration(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        public string ClientId => Configuration["ClientId"];
    }
}