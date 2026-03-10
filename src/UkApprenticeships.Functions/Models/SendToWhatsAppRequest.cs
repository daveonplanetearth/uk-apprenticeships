using System.Text.Json.Serialization;

namespace UkApprenticeships.Functions.Models;

public class SendToWhatsAppRequest
{
    [JsonPropertyName("vacancy")]
    public Vacancy Vacancy { get; set; } = null!;

    [JsonPropertyName("distanceKm")]
    public double DistanceKm { get; set; }

    [JsonPropertyName("mobile")]
    public string Mobile { get; set; } = null!;
}
