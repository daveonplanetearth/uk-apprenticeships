namespace UkApprenticeships.Functions.Configuration;

public class XApiOptions
{
    public const string SectionName = "XApi";

    public string ConsumerKey { get; set; } = null!;
    public string ConsumerSecret { get; set; } = null!;
    public string AccessToken { get; set; } = null!;
    public string AccessTokenSecret { get; set; } = null!;
}
