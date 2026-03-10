using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using UkApprenticeships.Functions.Models;
using UkApprenticeships.Functions.Services;

namespace UkApprenticeships.Functions.Functions;

public class SendToWhatsAppFunction
{
    private readonly IWhatsAppService _whatsAppService;
    private readonly ILogger<SendToWhatsAppFunction> _logger;

    public SendToWhatsAppFunction(
        IWhatsAppService whatsAppService,
        ILogger<SendToWhatsAppFunction> logger)
    {
        _whatsAppService = whatsAppService;
        _logger = logger;
    }

    [Function("SendToWhatsApp")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "SendToWhatsApp")] HttpRequestData req)
    {
        try
        {
            var body = await req.ReadAsStringAsync();

            if (string.IsNullOrEmpty(body))
            {
                _logger.LogError("Request body is empty");
                var badResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Request body cannot be empty");
                return badResponse;
            }

            var request = JsonSerializer.Deserialize<SendToWhatsAppRequest>(
                body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (request?.Vacancy == null)
            {
                _logger.LogError("Request body does not contain valid vacancy data");
                var badResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Request body must contain vacancy data");
                return badResponse;
            }

            if (string.IsNullOrEmpty(request.Mobile))
            {
                _logger.LogError("Request body does not contain mobile number");
                var badResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Request body must contain mobile number");
                return badResponse;
            }

            await _whatsAppService.SendVacancyAsync(request.Vacancy, request.DistanceKm, request.Mobile);

            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await response.WriteStringAsync("Message sent successfully");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SendToWhatsAppFunction");
            var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error: {ex.Message}");
            return errorResponse;
        }
    }
}
