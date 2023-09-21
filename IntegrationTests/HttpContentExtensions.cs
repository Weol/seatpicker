using System.Net.Http.Json;
using System.Text.Json;
using Seatpicker.Infrastructure;

namespace Seatpicker.IntegrationTests;

public static class HttpClientExtensions
{
    public static Task<T?> ReadAsJsonAsync<T>(this HttpContent httpContent)
    {
        var jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        JsonSerializationExtensions.ConfigureJsonOptions(jsonSerializerOptions);
        return httpContent.ReadFromJsonAsync<T>(jsonSerializerOptions);
    }
}