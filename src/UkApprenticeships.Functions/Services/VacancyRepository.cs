using Microsoft.Azure.Cosmos;
using UkApprenticeships.Functions.Configuration;
using UkApprenticeships.Functions.Models;
using Microsoft.Extensions.Options;

namespace UkApprenticeships.Functions.Services;

public class VacancyRepository : IVacancyRepository
{
    private readonly Container _container;

    public VacancyRepository(CosmosClient cosmosClient, IOptions<CosmosDbOptions> options)
    {
        var cosmosDbOptions = options.Value;
        var database = cosmosClient.GetDatabase(cosmosDbOptions.DatabaseName);
        _container = database.GetContainer(cosmosDbOptions.ContainerName);
    }

    public async Task UpsertVacancyAsync(CosmosVacancyDocument document)
    {
        await _container.UpsertItemAsync(
            document,
            new PartitionKey(document.PartitionKey),
            new ItemRequestOptions { EnableContentResponseOnWrite = false });
    }
}
