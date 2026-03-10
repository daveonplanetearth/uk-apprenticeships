using System.Text.Json.Serialization;

namespace UkApprenticeships.Functions.Models;

public class UserDocument
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;

    [JsonPropertyName("mobile")]
    public string Mobile { get; set; } = null!;

    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("radius")]
    public double Radius { get; set; }
}
