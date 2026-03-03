namespace WebScrapeRag.Models
{
    public class GitHubModelOptions
    {
        public string Token { get; set; } = string.Empty;
        public string ChatModel { get; set; } = string.Empty;
        public string EmbeddingModel { get; set; } = string.Empty;
    }
}
