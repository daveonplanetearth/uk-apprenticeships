using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using UkApprenticeships.Functions.Configuration;
using UkApprenticeships.Functions.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration((context, config) =>
    {
        // Ensure configuration is loaded from environment and settings
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        // Register configuration options
        services.Configure<ApprenticeshipApiOptions>(
            configuration.GetSection(ApprenticeshipApiOptions.SectionName));
        services.Configure<CosmosDbOptions>(
            configuration.GetSection(CosmosDbOptions.SectionName));

        // Register typed HttpClient for VacancyApiClient
        services.AddHttpClient<IVacancyApiClient, VacancyApiClient>((serviceProvider, client) =>
        {
            var apiOptions = serviceProvider.GetRequiredService<IOptions<ApprenticeshipApiOptions>>();
            var options = apiOptions.Value;

            client.BaseAddress = new Uri(options.BaseUrl);
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", options.SubscriptionKey);
            client.DefaultRequestHeaders.Add("X-Version", options.ApiVersion);
        });

        // Register CosmosClient as singleton (v3 SDK uses camelCase by default)
        services.AddSingleton(sp =>
        {
            var cosmosOptions = sp.GetRequiredService<IOptions<CosmosDbOptions>>();
            var options = cosmosOptions.Value;

            return new CosmosClient(options.ConnectionString);
        });

        // Register repository
        services.AddSingleton<IVacancyRepository, VacancyRepository>();
    })
    .Build();

await host.RunAsync();
