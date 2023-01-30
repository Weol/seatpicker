using System.Text.Json;
using Seatpicker.Application;
using Seatpicker.Infrastructure;
using Seatpicker.Infrastructure.Middleware;
using Seatpicker.Infrastructure.ModelValidation;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables("App_");

if (builder.Environment.IsEnvironment("local"))
{
    builder.Configuration.AddJsonFile("appsettings.local.json");
}

builder.Services.AddControllers(options =>
{
    options.Filters.Add<HttpResponseExceptionFilter>();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var jsonSerializerOptions = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,
};

builder.Services
    .AddSingleton(jsonSerializerOptions)
    .AddModelValidator()
    .AddAdapters(builder.Configuration)
    .AddApplication(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();