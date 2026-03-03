using UkApprenticeships.Functions.Models;

namespace UkApprenticeships.Functions.Services;

public interface IVacancyRepository
{
    Task UpsertVacancyAsync(CosmosVacancyDocument document);
}
