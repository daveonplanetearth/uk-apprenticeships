using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using UkApprenticeships.Functions.Configuration;
using UkApprenticeships.Functions.Models;

namespace UkApprenticeships.Functions.Services;

public class UserRepository : IUserRepository
{
    private readonly Container _container;

    public UserRepository(CosmosClient cosmosClient, IOptions<CosmosDbOptions> options)
    {
        var db = cosmosClient.GetDatabase(options.Value.DatabaseName);
        _container = db.GetContainer("Users");
    }

    public async Task<IReadOnlyList<UserDocument>> GetAllUsersAsync()
    {
        var iterator = _container.GetItemQueryIterator<UserDocument>("SELECT * FROM c WHERE c.isEnabled = true");
        var results = new List<UserDocument>();
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            results.AddRange(response);
        }
        return results.AsReadOnly();
    }
}
