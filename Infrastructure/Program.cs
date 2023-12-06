using Seatpicker.Application;
using Seatpicker.Infrastructure;
using Seatpicker.Infrastructure.Adapters;
using Seatpicker.Infrastructure.Authentication;
using Seatpicker.Infrastructure.Entrypoints;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment()) builder.Configuration.AddJsonFile("appsettings.local.json");

builder.Configuration.AddEnvironmentVariables("App_");
builder.Configuration.AddSeatpickerKeyvault();

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
app.UseAdapters();

app.Run();

namespace Seatpicker.Infrastructure
{
    public class Program
    {
    }
}