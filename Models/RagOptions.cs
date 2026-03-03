namespace WebScrapeRag.Models
{
    public class RagOptions
    {
        public int ChunkSize { get; set; } = 300;
        public int ChunkOverlap { get; set; } = 50;
        public int TopK { get; set; } = 3;
        public float Temperature { get; set; } = 0.4F;
        public int MaxTokens { get; set; } = 1200;

    }
}
