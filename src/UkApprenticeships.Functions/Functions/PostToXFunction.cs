using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using UkApprenticeships.Functions.Services;

namespace UkApprenticeships.Functions.Functions;

public class PostToXFunction
{
    private readonly IXApiService _xApiService;
    private readonly ILogger<PostToXFunction> _logger;

    public PostToXFunction(
        IXApiService xApiService,
        ILogger<PostToXFunction> logger)
    {
        _xApiService = xApiService;
        _logger = logger;
    }

    [Function("PostToX")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "PostToX")] HttpRequestData req)
    {
        try
        {
            var body = await req.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(body))
            {
                _logger.LogError("Request body is empty");
                var badResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Request body cannot be empty");
                return badResponse;
            }

            await _xApiService.PostTweetAsync(body.Trim());

            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await response.WriteStringAsync("Tweet posted successfully");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in PostToXFunction");
            var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error: {ex.Message}");
            return errorResponse;
        }
    }
}
