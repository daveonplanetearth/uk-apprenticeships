using System.Text.Json.Serialization;

namespace UkApprenticeships.Functions.Models;

public class VacancyApiResponse
{
    [JsonPropertyName("vacancies")]
    public List<Vacancy>? Vacancies { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("totalFiltered")]
    public int TotalFiltered { get; set; }

    [JsonPropertyName("totalPages")]
    public int TotalPages { get; set; }
}
