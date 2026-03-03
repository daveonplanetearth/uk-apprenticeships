namespace UkApprenticeships.Functions.Configuration;

public class ApprenticeshipApiOptions
{
    public const string SectionName = "ApprenticeshipApi";

    public string BaseUrl { get; set; } = null!;
    public string SubscriptionKey { get; set; } = null!;
    public string ApiVersion { get; set; } = null!;
    public int PostedInLastNumberOfDays { get; set; }
    public int PageSize { get; set; }
}
