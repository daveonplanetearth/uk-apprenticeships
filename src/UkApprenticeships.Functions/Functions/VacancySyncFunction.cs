using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UkApprenticeships.Functions.Configuration;
using UkApprenticeships.Functions.Models;
using UkApprenticeships.Functions.Services;

namespace UkApprenticeships.Functions.Functions;

public class VacancySyncFunction
{
    private readonly IVacancyApiClient _apiClient;
    private readonly IVacancyRepository _repository;
    private readonly ApprenticeshipApiOptions _apiOptions;
    private readonly ILogger<VacancySyncFunction> _logger;

    public VacancySyncFunction(
        IVacancyApiClient apiClient,
        IVacancyRepository repository,
        IOptions<ApprenticeshipApiOptions> apiOptions,
        ILogger<VacancySyncFunction> logger)
    {
        _apiClient = apiClient;
        _repository = repository;
        _apiOptions = apiOptions.Value;
        _logger = logger;
    }

    [Function("VacancySyncFunction")]
    public async Task Run(
#if DEBUG
        [TimerTrigger("%VacancySyncFunctionInterval%", UseMonitor = false, RunOnStartup = true)] TimerInfo myTimer)
#else
        [TimerTrigger("%VacancySyncFunctionInterval%", UseMonitor = false, RunOnStartup = false)] TimerInfo myTimer)
#endif
    {
        _logger.LogInformation($"VacancySyncFunction started at {DateTime.UtcNow}");

        int currentPage = 1;
        int totalPages = 1;
        int totalUpserted = 0;
        int totalErrors = 0;

        try
        {
            do
            {
                _logger.LogInformation($"Fetching page {currentPage} of {totalPages}");

                var response = await _apiClient.GetVacanciesAsync(
                    currentPage,
                    _apiOptions.PageSize,
                    _apiOptions.PostedInLastNumberOfDays);

                if (response == null)
                {
                    _logger.LogError("API response is null");
                    break;
                }

                totalPages = response.TotalPages;

                if (response.Vacancies != null && response.Vacancies.Count > 0)
                {
                    foreach (var vacancy in response.Vacancies)
                    {
                        try
                        {
                            var document = CosmosVacancyDocument.FromVacancy(vacancy);
                            await _repository.UpsertVacancyAsync(document);
                            totalUpserted++;
                        }
                        catch (CosmosException ex)
                        {
                            totalErrors++;
                            _logger.LogError(
                                ex,
                                $"Failed to upsert vacancy {vacancy.VacancyReference}: {ex.Message}");
                        }
                        catch (Exception ex)
                        {
                            totalErrors++;
                            _logger.LogError(
                                ex,
                                $"Unexpected error upserting vacancy {vacancy.VacancyReference}: {ex.Message}");
                        }
                    }
                }

                currentPage++;
            } while (currentPage <= totalPages);

            _logger.LogInformation(
                $"VacancySyncFunction completed. Upserted: {totalUpserted}, Errors: {totalErrors}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in VacancySyncFunction");
            throw;
        }
    }
}
