using Microsoft.IdentityModel.Logging;
using Seatpicker.Application;
using Seatpicker.Infrastructure;
using Seatpicker.Infrastructure.Adapters;
using Seatpicker.Infrastructure.Authentication;
using Seatpicker.Infrastructure.Entrypoints;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.local.json", true);

builder.Configuration.AddEnvironmentVariables("App_");
builder.Configuration.AddSeatpickerKeyvault();

IdentityModelEventSource.ShowPII = true;

builder.Services.AddApplicationInsightsTelemetry()
    .AddLogging()
    .AddAdapters()
    .AddSeatpickerAuthentication()
    .ConfigureJsonSerialization()
    .AddEntrypoints(builder.Configuration)
    .AddApplication()
    .AddSwaggerGen();

var app = builder.Build();

app.UseSwaggerGen();
app.UseRouting();
app.UseSeatpickerAuthentication();
app.UseEntrypoints();

app.Run();

namespace Seatpicker.Infrastructure
{
    public class Program;
}