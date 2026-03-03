using System.Text.Json.Serialization;

namespace UkApprenticeships.Functions.Models;

public class VacancyAddress
{
    [JsonPropertyName("addressLine1")]
    public string? AddressLine1 { get; set; }

    [JsonPropertyName("addressLine2")]
    public string? AddressLine2 { get; set; }

    [JsonPropertyName("addressLine3")]
    public string? AddressLine3 { get; set; }

    [JsonPropertyName("addressLine4")]
    public string? AddressLine4 { get; set; }

    [JsonPropertyName("town")]
    public string? Town { get; set; }

    [JsonPropertyName("postcode")]
    public string? Postcode { get; set; }

    [JsonPropertyName("county")]
    public string? County { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }
}
