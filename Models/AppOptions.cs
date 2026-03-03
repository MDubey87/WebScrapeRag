namespace WebScrapeRag.Models
{
    public class AppOptions
    {
        public GitHubModelOptions GitHubModel { get; set; } = new GitHubModelOptions();
        public RagOptions Rag { get; set; } = new RagOptions();
    }
}
