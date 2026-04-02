using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;
using UkApprenticeships.Functions.Models;
using UkApprenticeships.Functions.Services;

namespace UkApprenticeships.Functions.Functions;

public class ProcessVacanciesFunction
{
    private readonly IVacancyRepository _repository;
    private readonly IUserRepository _userRepository;
    private readonly IWhatsAppService _whatsAppService;
    private readonly IXApiService _xApiService;
    private readonly IFlux2Service _flux2Service;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ProcessVacanciesFunction> _logger;

    public ProcessVacanciesFunction(
        IVacancyRepository repository,
        IUserRepository userRepository,
        IWhatsAppService whatsAppService,
        IXApiService xApiService,
        IFlux2Service flux2Service,
        IConfiguration configuration,
        ILogger<ProcessVacanciesFunction> logger)
    {
        _repository = repository;
        _userRepository = userRepository;
        _whatsAppService = whatsAppService;
        _xApiService = xApiService;
        _flux2Service = flux2Service;
        _configuration = configuration;
        _logger = logger;
    }

    [Function("ProcessVacanciesFunction")]
    public async Task Run(
#if DEBUG
        [TimerTrigger("%ProcessVacanciesFunctionInterval%", UseMonitor = false, RunOnStartup = true)] TimerInfo myTimer)
#else
        [TimerTrigger("%ProcessVacanciesFunctionInterval%", UseMonitor = false, RunOnStartup = false)] TimerInfo myTimer)
#endif
    {
        _logger.LogInformation($"ProcessVacanciesFunction started at {DateTime.UtcNow}");

        try
        {
            var partitionKey = DateTime.UtcNow.ToString("yyyyMMdd");
            var vacancies = await _repository.GetVacanciesByDateAsync(partitionKey);

            _logger.LogInformation($"Retrieved {vacancies.Count} vacancies for partition key {partitionKey}");

            var rateLimitPerMinute = _configuration.GetValue<int>("ProcessVacanciesRateLimitPerMinute");

            var windowStart = DateTime.UtcNow;
            var processedInWindow = 0;

            foreach (var doc in vacancies)
            {
                if (rateLimitPerMinute > 0 && processedInWindow >= rateLimitPerMinute)
                {
                    var windowElapsed = DateTime.UtcNow - windowStart;
                    var waitTime = TimeSpan.FromMinutes(1) - windowElapsed;
                    if (waitTime > TimeSpan.Zero)
                    {
                        _logger.LogInformation($"Rate limit reached ({rateLimitPerMinute}/min). Waiting {waitTime.TotalSeconds:F1}s.");
                        await Task.Delay(waitTime);
                    }
                    windowStart = DateTime.UtcNow;
                    processedInWindow = 0;
                }

                var v = doc.Vacancy;
                var postcode = v.Addresses?.FirstOrDefault()?.Postcode;
                var closing = v.ClosingDate.HasValue ? v.ClosingDate.Value.ToString("dd MMM yyyy") : null;

                var parts = new List<string>();
                if (!string.IsNullOrWhiteSpace(v.Title) && !string.IsNullOrWhiteSpace(v.EmployerName))
                    parts.Add($"{v.Title} @ {v.EmployerName}");
                else if (!string.IsNullOrWhiteSpace(v.Title))
                    parts.Add(v.Title!);

                var details = string.Join("  ", new[] {
                    postcode != null ? $"📍 {postcode}" : null,
                    closing != null ? $"⏰ Closes {closing}" : null
                }.Where(s => s != null));
                if (!string.IsNullOrEmpty(details))
                    parts.Add(details);

                if (!string.IsNullOrWhiteSpace(v.VacancyUrl))
                    parts.Add(v.VacancyUrl!);

                var tweetText = string.Join("\n", parts);

                try
                {
                    var imageStream = await _flux2Service.GenerateImageAsync(tweetText, width: 610, height: 300);
                    await _xApiService.PostTweetMediaAsync(tweetText, imageStream);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to post tweet for vacancy {v.VacancyReference}");
                }

                processedInWindow++;
            }

            //var users = await _userRepository.GetAllUsersAsync();
            //_logger.LogInformation($"Retrieved {users.Count} users");

            //foreach (var user in users)
            //{
            //    _logger.LogInformation($"Processing vacancies for user {user.Id} with mobile {user.Mobile}");

            //    var filtered = new List<(CosmosVacancyDocument Document, double MinDistanceKm)>();

            //    foreach (var vacancy in vacancies)
            //    {
            //        if (vacancy.Vacancy.Addresses == null || vacancy.Vacancy.Addresses.Count == 0)
            //            continue;

            //        foreach (var address in vacancy.Vacancy.Addresses)
            //        {
            //            if (address.Latitude == null || address.Longitude == null)
            //                continue;

            //            var distance = CalculateDistanceKm(
            //                address.Latitude.Value,
            //                address.Longitude.Value,
            //                user.Latitude,
            //                user.Longitude);

            //            if (distance <= user.Radius)
            //            {
            //                filtered.Add((vacancy, distance));
            //            }
            //        }
            //    }

            //    var sorted = filtered.OrderBy(f => f.MinDistanceKm).ToList();

            //    _logger.LogInformation($"Found {sorted.Count} vacancies within {user.Radius}km of user {user.Id}'s location");

            //    foreach (var item in sorted)
            //    {
            //        var logEntry = new
            //        {
            //            VacancyReference = item.Document.Vacancy.VacancyReference,
            //            Title = item.Document.Vacancy.Title,
            //            EmployerName = item.Document.Vacancy.EmployerName,
            //            ClosingDate = item.Document.Vacancy.ClosingDate,
            //            DistanceKm = Math.Round(item.MinDistanceKm, 2),
            //            PostedDate = item.Document.Vacancy.PostedDate,
            //            NumberOfPositions = item.Document.Vacancy.NumberOfPositions,
            //            ApprenticeshipLevel = item.Document.Vacancy.ApprenticeshipLevel,
            //            CourseTitle = item.Document.Vacancy.Course?.Title,
            //            VacancyUrl = item.Document.Vacancy.VacancyUrl,
            //            AddressLine1 = item.Document.Vacancy.Addresses?.FirstOrDefault()?.AddressLine1,
            //            AddressLine2 = item.Document.Vacancy.Addresses?.FirstOrDefault()?.AddressLine2,
            //            AddressLine3 = item.Document.Vacancy.Addresses?.FirstOrDefault()?.AddressLine3,
            //            AddressLine4 = item.Document.Vacancy.Addresses?.FirstOrDefault()?.AddressLine4,
            //            Postcode = item.Document.Vacancy.Addresses?.FirstOrDefault()?.Postcode
            //        };

            //        _logger.LogInformation(
            //            $"Vacancy: {System.Text.Json.JsonSerializer.Serialize(logEntry)}");

            //        try
            //        {
            //            await _whatsAppService.SendVacancyAsync(item.Document.Vacancy, Math.Round(item.MinDistanceKm, 2), user.Mobile);
            //        }
            //        catch (Exception ex)
            //        {
            //            _logger.LogError(ex, $"Failed to send WhatsApp message for vacancy {item.Document.Vacancy.VacancyReference} to user {user.Id}");
            //        }
            //    }
            //}
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Fatal error in ProcessVacanciesFunction. Exception: {ExceptionType}, Message: {ExceptionMessage}, StackTrace: {ExceptionStackTrace}, InnerException: {InnerException}",
                ex.GetType().FullName,
                ex.Message,
                ex.StackTrace,
                ex.InnerException?.ToString());
            throw;
        }
    }

    private static double CalculateDistanceKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadiusKm = 6371;

        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return earthRadiusKm * c;
    }

    private static double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }
}
