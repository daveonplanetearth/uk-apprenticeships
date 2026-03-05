using System.Text.Json.Serialization;

namespace UkApprenticeships.Functions.Models;

public class VacancyCourse
{
    [JsonPropertyName("larsCode")]
    public int? LarsCode { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("level")]
    public int? Level { get; set; }

    [JsonPropertyName("route")]
    public string? Route { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }
}
