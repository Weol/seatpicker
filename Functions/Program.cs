using System.Text.Json;
using Application;
using Application.Middleware;
using Functions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Seatpicker.Adapters;
using Seatpicker.Domain;

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
            .AddApplication(config)
            .AddAdapters(config)
            .AddUserContext();
    })
    .Build();

host.Run();