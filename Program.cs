using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebScrapeRag;
using WebScrapeRag.Models;
using WebScrapeRag.Services;

await Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(cfg =>
    {
        cfg.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
           .AddEnvironmentVariables();
    }).ConfigureServices((cfg, services) =>
    {
        services.Configure<AppOptions>(cfg.Configuration);
        services.AddSingleton<HttpClient>(sp =>
        {
            var http = new HttpClient();

            http.DefaultRequestHeaders.UserAgent.ParseAdd(
                            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36"
                        );
            return http;

        });
        services.AddSingleton<WebsiteTextExtractorService>();
        services.AddSingleton<TextChunkerService>();
        services.AddSingleton<InMemoryVectorStoreService>();
        services.AddSingleton<OpenAiClientService>();
        services.AddSingleton<ConversationMemory>();
        services.AddSingleton<RagPipeline>();
        services.AddHostedService<ConsoleApp>();
    }).RunConsoleAsync();