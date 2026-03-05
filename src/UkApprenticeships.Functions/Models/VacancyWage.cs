using System.Text.Json.Serialization;

namespace UkApprenticeships.Functions.Models;

public class VacancyWage
{
    [JsonPropertyName("wageType")]
    public string? WageType { get; set; }

    [JsonPropertyName("wageAmount")]
    public decimal? WageAmount { get; set; }

    [JsonPropertyName("wageUnit")]
    public string? WageUnit { get; set; }

    [JsonPropertyName("wageAdditionalInformation")]
    public string? WageAdditionalInformation { get; set; }

    [JsonPropertyName("workingWeekDescription")]
    public string? WorkingWeekDescription { get; set; }
}
