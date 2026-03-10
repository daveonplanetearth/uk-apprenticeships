using System.Text.Json;
using Microsoft.Extensions.Options;
using UkApprenticeships.Functions.Configuration;
using UkApprenticeships.Functions.Models;

namespace UkApprenticeships.Functions.Services;

public class WhatsAppService : IWhatsAppService
{
    private readonly HttpClient _httpClient;
    private readonly WhatsAppOptions _options;

    public WhatsAppService(HttpClient httpClient, IOptions<WhatsAppOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task SendVacancyAsync(Vacancy vacancy, double distanceKm, string mobile)
    {
        var closingDateText = GetClosingDateText(vacancy.ClosingDate);

        var payload = new
        {
            messaging_product = "whatsapp",
            to = mobile,
            type = "template",
            template = new
            {
                name = _options.TemplateName,
                language = new { code = "en" },
                components = new[]
                {
                    new
                    {
                        type = "body",
                        parameters = new[]
                        {
                            new { type = "text", text = vacancy.Title ?? string.Empty },
                            new { type = "text", text = vacancy.EmployerName ?? string.Empty },
                            new { type = "text", text = closingDateText },
                            new { type = "text", text = vacancy.VacancyUrl ?? string.Empty },
                            new { type = "text", text = $"{distanceKm}km" }
                        }
                    }
                }
            }
        };

        var content = new StringContent(
            JsonSerializer.Serialize(payload),
            System.Text.Encoding.UTF8,
            "application/json");

        var url = new Uri($"{_options.ApiVersion}/{_options.PhoneNumberId}/messages", UriKind.Relative);
        var response = await _httpClient.PostAsync(url, content);
        response.EnsureSuccessStatusCode();
    }

    private static string GetClosingDateText(DateTime? closingDate)
    {
        if (!closingDate.HasValue)
            return string.Empty;

        var daysRemaining = (closingDate.Value.Date - DateTime.Today).Days;
        return daysRemaining switch
        {
            < 0 => "Closed",
            0 => "Today",
            1 => "1 day",
            _ => $"{daysRemaining} days"
        };
    }
}
