using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Seatpicker.Application;
using Seatpicker.Infrastructure;
using Seatpicker.Infrastructure.Middleware;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(builder =>
    {
        builder.UseWhen<ModelValidationExceptionHandlerMiddleware>(context => context.IsHttpTrigger());
        builder.UseWhen<JsonExceptionHandlerMiddleware>(context => context.IsHttpTrigger());
    })
    .ConfigureAppConfiguration(builder =>
    {
        builder.AddJsonFile("appsettings.json");
        builder.AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        var config = context.Configuration;

        var jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };
        
        services
            .AddSingleton(jsonSerializerOptions)
            .AddSingleton<IRequestModelDeserializerService, RequestModelDeserializerService>()
            .AddSingleton<IResponseModelSerializerService, ResponseModelSerializerService>()
            .AddAdapters(config)
            .AddApplication(config);
    })
    .Build();

host.Run();