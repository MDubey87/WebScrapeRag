using Microsoft.Extensions.Hosting;
using WebScrapeRag.Services;

namespace WebScrapeRag
{
    sealed class ConsoleApp : IHostedService
    {
        private readonly RagPipeline _rag;
        public ConsoleApp(RagPipeline rag)
        {
            _rag = rag;
        }
        public async Task StartAsync(CancellationToken ct)
        {
            Console.WriteLine("Welcome to the Enhanced Web Scraping RAG Pipeline (.NET Console).");

            while (true)
            {
                Console.Write("\nEnter website URL (or 'quit'): ");
                var url = Console.ReadLine()?.Trim();

                if (string.Equals(url, "quit", StringComparison.OrdinalIgnoreCase))
                    break;

                if (string.IsNullOrWhiteSpace(url))
                    continue;

                try
                {
                    await _rag.LoadWebsiteAsync(url, ct);

                    Console.WriteLine("\nRAG initialized. Enter questions.");
                    Console.WriteLine("Type 'new' to load a new website, 'quit' to exit.");

                    while (true)
                    {
                        Console.Write("\nQuery: ");
                        var q = Console.ReadLine()?.Trim();

                        if (string.Equals(q, "quit", StringComparison.OrdinalIgnoreCase))
                            return;

                        if (string.Equals(q, "new", StringComparison.OrdinalIgnoreCase))
                            break;

                        if (string.IsNullOrWhiteSpace(q))
                            continue;

                        var answer = await _rag.AskAsync(q, ct);
                        Console.WriteLine($"\nRAG Response:\n{answer}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    Console.WriteLine("Try another URL (some sites require JavaScript rendering).");
                }
            }

            Console.WriteLine("Goodbye!");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
