using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace WebhookFunctionApp.Services.EndpointProxy;

public class ApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ApiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<string> PostJsonWithFactoryAsync<T>(
        string url,
        T data,
        Dictionary<string, string>? headers = null)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();

            string jsonContent = JsonSerializer.Serialize(data);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            var response = await client.SendAsync(request);
            
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"HTTP request failed: {ex.Message}", ex);
        }
    }
}