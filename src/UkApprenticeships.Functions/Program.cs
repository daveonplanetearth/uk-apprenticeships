using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UkApprenticeships.Functions.Configuration;
using UkApprenticeships.Functions.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureLogging(logging =>
    {
        logging.AddFilter("Microsoft.Extensions.Logging.ApplicationInsights", LogLevel.Information);
    })
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

        // Register Application Insights with exception details
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.Configure<LoggerFilterOptions>(options =>
        {
            // By default, the Functions SDK adds a filter that only sends Warning+ to App Insights.
            // Remove it so our ConfigureLogging filter takes precedence and exception details are preserved.
            // The SDK adds a default filter that sets Warning as the minimum level for App Insights,
            // which suppresses exception details from Information-level logs. Remove it.
            var defaultRule = options.Rules.FirstOrDefault(rule =>
                rule.ProviderName == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");
            if (defaultRule is not null)
            {
                options.Rules.Remove(defaultRule);
            }
        });

        // Register repositories
        services.AddSingleton<IVacancyRepository, VacancyRepository>();
        services.AddSingleton<IUserRepository, UserRepository>();
    })
    .Build();

await host.RunAsync();
