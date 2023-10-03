using Seatpicker.Application;
using Seatpicker.Infrastructure;
using Seatpicker.Infrastructure.Adapters;
using Seatpicker.Infrastructure.Authentication;
using Seatpicker.Infrastructure.Entrypoints;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment()) builder.Configuration.AddJsonFile("appsettings.local.json");

builder.Configuration
    .AddSeatpickerKeyvault()
    .AddEnvironmentVariables("App_");

builder.Services.AddApplicationInsightsTelemetry()
    .AddAdapters()
    .AddSeatpickerAuthentication()
    .ConfigureJsonSerialization()
    .AddEntrypoints(builder.Configuration)
    .AddApplication()
    .AddSwaggerGen();

var app = builder.Build();

app.UseSwaggerGen();
app.UseEntrypoints();
app.UseSeatpickerAuthentication();

app.Run();

namespace Seatpicker.Infrastructure
{
    public partial class Program
    {
    }
}