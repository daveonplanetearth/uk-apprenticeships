using UkApprenticeships.Functions.Models;

namespace UkApprenticeships.Functions.Services;

public interface IVacancyApiClient
{
    Task<VacancyApiResponse?> GetVacanciesAsync(int pageNumber, int pageSize, int postedInLastNumberOfDays);
}
