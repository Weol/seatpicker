using Seatpicker.Application;
using Seatpicker.Infrastructure;
using Seatpicker.Infrastructure.Adapters;
using Seatpicker.Infrastructure.Authentication;
using Seatpicker.Infrastructure.Entrypoints;
using Seatpicker.Infrastructure.Entrypoints.Http.Utils;

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
    .AddApplication();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseEntrypoints();
app.UseSeatpickerAuthentication();

app.Run();

namespace Seatpicker.Infrastructure
{
    public partial class Program
    {
    }
}