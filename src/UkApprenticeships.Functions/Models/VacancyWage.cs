using System.Text.Json.Serialization;

namespace UkApprenticeships.Functions.Models;

public class VacancyWage
{
    [JsonPropertyName("minimumAnnualWage")]
    public decimal? MinimumAnnualWage { get; set; }

    [JsonPropertyName("maximumAnnualWage")]
    public decimal? MaximumAnnualWage { get; set; }

    [JsonPropertyName("wageType")]
    public string? WageType { get; set; }

    [JsonPropertyName("currencyCode")]
    public string? CurrencyCode { get; set; }
}
