using System.Net.Http.Json;
using System.Text.Json;
using UkApprenticeships.Functions.Models;

namespace UkApprenticeships.Functions.Services;

public class VacancyApiClient : IVacancyApiClient
{
    private readonly HttpClient _httpClient;

    public VacancyApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<VacancyApiResponse?> GetVacanciesAsync(
        int pageNumber,
        int pageSize,
        int postedInLastNumberOfDays)
    {
        var relativePath = $"vacancies/vacancy?PostedInLastNumberOfDays={postedInLastNumberOfDays}&PageNumber={pageNumber}&PageSize={pageSize}";

        try
        {
            var response = await _httpClient.GetAsync(relativePath);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var apiResponse = JsonSerializer.Deserialize<VacancyApiResponse>(json, jsonOptions);
            return apiResponse;
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Failed to fetch vacancies from API: {ex.Message}", ex);
        }
    }
}
