using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using UkApprenticeships.Functions.Configuration;

namespace UkApprenticeships.Functions.Services;

public class Flux2Service : IFlux2Service
{
    private const string Endpoint = "/providers/blackforestlabs/v1/flux-2-pro?api-version=preview";

    private readonly HttpClient _httpClient;
    private readonly Flux2Options _options;

    public Flux2Service(HttpClient httpClient, IOptions<Flux2Options> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<Stream> GenerateImageAsync(string prompt, int width = 1024, int height = 1024)
    {
        var payload = JsonSerializer.Serialize(new
        {
            prompt,
            width,
            height,
            n = 1,
            model = "FLUX.2-pro"
        });

        using var request = new HttpRequestMessage(HttpMethod.Post, Endpoint);
        request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {_options.ApiKey}");
        request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var b64 = doc.RootElement.GetProperty("data")[0].GetProperty("b64_json").GetString()!;

        return new MemoryStream(Convert.FromBase64String(b64));
    }
}
