using System.Text.Json.Serialization;

namespace UkApprenticeships.Functions.Models;

public class VacancyCourse
{
    [JsonPropertyName("id")]
    public int? Id { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("level")]
    public string? Level { get; set; }

    [JsonPropertyName("route")]
    public string? Route { get; set; }
}
