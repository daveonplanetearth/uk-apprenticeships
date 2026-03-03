using System.Text.Json.Serialization;

namespace UkApprenticeships.Functions.Models;

public class CosmosVacancyDocument
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;

    [JsonPropertyName("partitionKey")]
    public string PartitionKey { get; set; } = null!;

    [JsonPropertyName("vacancy")]
    public Vacancy Vacancy { get; set; } = null!;

    [JsonPropertyName("syncedAtUtc")]
    public DateTime SyncedAtUtc { get; set; }

    public static CosmosVacancyDocument FromVacancy(Vacancy vacancy)
    {
        return new CosmosVacancyDocument
        {
            Id = vacancy.VacancyReference ?? throw new ArgumentException("VacancyReference cannot be null"),
            PartitionKey = vacancy.VacancyReference,
            Vacancy = vacancy,
            SyncedAtUtc = DateTime.UtcNow
        };
    }
}
