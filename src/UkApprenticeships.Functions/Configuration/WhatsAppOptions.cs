namespace UkApprenticeships.Functions.Configuration;

public class WhatsAppOptions
{
    public const string SectionName = "WhatsApp";
    public string ApiVersion { get; set; } = "v22.0";
    public string PhoneNumberId { get; set; } = null!;
    public string AccessToken { get; set; } = null!;
    public string RecipientNumber { get; set; } = null!;
    public string TemplateName { get; set; } = null!;
}
