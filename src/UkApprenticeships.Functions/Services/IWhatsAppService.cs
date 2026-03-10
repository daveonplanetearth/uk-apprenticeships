using UkApprenticeships.Functions.Models;

namespace UkApprenticeships.Functions.Services;

public interface IWhatsAppService
{
    Task SendVacancyAsync(Vacancy vacancy, double distanceKm, string mobile);
}
