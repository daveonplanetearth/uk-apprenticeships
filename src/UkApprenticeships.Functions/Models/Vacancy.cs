using System.Text.Json.Serialization;

namespace UkApprenticeships.Functions.Models;

public class Vacancy
{
    [JsonPropertyName("vacancyReference")]
    public string? VacancyReference { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("postedDate")]
    public DateTime? PostedDate { get; set; }

    [JsonPropertyName("closingDate")]
    public DateTime? ClosingDate { get; set; }

    [JsonPropertyName("wage")]
    public VacancyWage? Wage { get; set; }

    [JsonPropertyName("addresses")]
    public List<VacancyAddress>? Addresses { get; set; }

    [JsonPropertyName("course")]
    public VacancyCourse? Course { get; set; }

    [JsonPropertyName("employerName")]
    public string? EmployerName { get; set; }

    [JsonPropertyName("trainingProvider")]
    public string? TrainingProvider { get; set; }

    [JsonPropertyName("numberOfPositions")]
    public int? NumberOfPositions { get; set; }

    [JsonPropertyName("apprenticeshipLevel")]
    public string? ApprenticeshipLevel { get; set; }

    [JsonPropertyName("applicationUrl")]
    public string? ApplicationUrl { get; set; }

    [JsonPropertyName("workingWeekDescription")]
    public string? WorkingWeekDescription { get; set; }

    [JsonPropertyName("hoursPerWeek")]
    public double? HoursPerWeek { get; set; }

    [JsonPropertyName("isDisabilityConfident")]
    public bool? IsDisabilityConfident { get; set; }

    [JsonPropertyName("startDate")]
    public DateTime? StartDate { get; set; }
}
