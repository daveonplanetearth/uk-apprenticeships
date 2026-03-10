using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using UkApprenticeships.Functions.Configuration;
using UkApprenticeships.Functions.Services;
using System.Text.Json;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("local.settings.json", optional: true, reloadOnChange: false);
        config.AddUserSecrets<Program>();
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
        services.Configure<WhatsAppOptions>(
            configuration.GetSection(WhatsAppOptions.SectionName));

        // Register typed HttpClient for VacancyApiClient
        services.AddHttpClient<IVacancyApiClient, VacancyApiClient>((serviceProvider, client) =>
        {
            var apiOptions = serviceProvider.GetRequiredService<IOptions<ApprenticeshipApiOptions>>();
            var options = apiOptions.Value;

            client.BaseAddress = new Uri(options.BaseUrl);
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", options.SubscriptionKey);
            client.DefaultRequestHeaders.Add("X-Version", options.ApiVersion);
        });

        // Register typed HttpClient for WhatsAppService
        services.AddHttpClient<IWhatsAppService, WhatsAppService>((serviceProvider, client) =>
        {
            var whatsAppOptions = serviceProvider.GetRequiredService<IOptions<WhatsAppOptions>>();
            var options = whatsAppOptions.Value;

            client.BaseAddress = new Uri("https://graph.facebook.com/");
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", options.AccessToken);
        });

        // Register CosmosClient as singleton with System.Text.Json serializer
        services.AddSingleton(sp =>
        {
            var cosmosOptions = sp.GetRequiredService<IOptions<CosmosDbOptions>>();
            var options = cosmosOptions.Value;

            return new CosmosClient(options.ConnectionString, new CosmosClientOptions
            {
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                }
            });
        });

        // Register repositories
        services.AddSingleton<IVacancyRepository, VacancyRepository>();
        services.AddSingleton<IUserRepository, UserRepository>();
    })
    .Build();

await host.RunAsync();
