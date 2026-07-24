using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Current.Api.Tests.Helpers;

public static class HttpClientExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public static Task<HttpResponseMessage> PostJsonAsync<TRequest>(
        this HttpClient httpClient,
        string requestUri,
        TRequest requestBody)
    {
        return httpClient.PostAsJsonAsync(requestUri, requestBody, JsonOptions);
    }

    public static async Task<TResponse?> ReadJsonAsync<TResponse>(this HttpResponseMessage response)
    {
        var responseJson = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(responseJson))
        {
            return default;
        }

        return JsonSerializer.Deserialize<TResponse>(responseJson, JsonOptions);
    }
}
