using System.Text.Json.Serialization;

namespace UkApprenticeships.Functions.Models;

public class Vacancy
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("numberOfPositions")]
    public int? NumberOfPositions { get; set; }

    [JsonPropertyName("postedDate")]
    public DateTime? PostedDate { get; set; }

    [JsonPropertyName("closingDate")]
    public DateTime? ClosingDate { get; set; }

    [JsonPropertyName("startDate")]
    public DateTime? StartDate { get; set; }

    [JsonPropertyName("wage")]
    public VacancyWage? Wage { get; set; }

    [JsonPropertyName("hoursPerWeek")]
    public double? HoursPerWeek { get; set; }

    [JsonPropertyName("expectedDuration")]
    public string? ExpectedDuration { get; set; }

    [JsonPropertyName("addresses")]
    public List<VacancyAddress>? Addresses { get; set; }

    [JsonPropertyName("applicationUrl")]
    public string? ApplicationUrl { get; set; }

    [JsonPropertyName("distance")]
    public double? Distance { get; set; }

    [JsonPropertyName("employerName")]
    public string? EmployerName { get; set; }

    [JsonPropertyName("employerWebsiteUrl")]
    public string? EmployerWebsiteUrl { get; set; }

    [JsonPropertyName("employerContactName")]
    public string? EmployerContactName { get; set; }

    [JsonPropertyName("employerContactPhone")]
    public string? EmployerContactPhone { get; set; }

    [JsonPropertyName("employerContactEmail")]
    public string? EmployerContactEmail { get; set; }

    [JsonPropertyName("course")]
    public VacancyCourse? Course { get; set; }

    [JsonPropertyName("apprenticeshipLevel")]
    public string? ApprenticeshipLevel { get; set; }

    [JsonPropertyName("providerName")]
    public string? ProviderName { get; set; }

    [JsonPropertyName("ukprn")]
    public int? Ukprn { get; set; }

    [JsonPropertyName("isDisabilityConfident")]
    public bool? IsDisabilityConfident { get; set; }

    [JsonPropertyName("vacancyUrl")]
    public string? VacancyUrl { get; set; }

    [JsonPropertyName("vacancyReference")]
    public string? VacancyReference { get; set; }

    [JsonPropertyName("isNationalVacancy")]
    public bool? IsNationalVacancy { get; set; }

    [JsonPropertyName("isNationalVacancyDetails")]
    public string? IsNationalVacancyDetails { get; set; }
}
