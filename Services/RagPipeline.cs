using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Text;
using WebScrapeRag.Models;

namespace WebScrapeRag.Services
{
    sealed class RagPipeline
    {
        private readonly WebsiteTextExtractorService _extractor;
        private readonly TextChunkerService _chunker;
        private readonly OpenAiClientService _openai;
        private readonly ConversationMemory _memory;
        private readonly RagOptions _ragOptions;

        private InMemoryVectorStoreService? _store;
        private List<string>? _chunks;

        private const string PROMPT_TEMPLATE = """
            Context: {context}

            Chat History:
            {history}

            Question: {question}

            Answer the question concisely based only on the given context. If the context doesn't contain relevant information, say "I don't have enough information to answer that question."

            But, if the question is generic, then go ahead and answer the question, example what is a electric vehicle?
            """;

        public RagPipeline(
            WebsiteTextExtractorService extractor,
            TextChunkerService chunker,
            OpenAiClientService openai,
            ConversationMemory memory,
            IOptions<AppOptions> appOptions)
        {
            _extractor = extractor;
            _chunker = chunker;
            _openai = openai;
            _memory = memory;
            _ragOptions = appOptions.Value.Rag?? new RagOptions();
        }

        public async Task LoadWebsiteAsync(string url, CancellationToken ct)
        {
            Console.WriteLine("Fetching and extracting website text...");
            var blocks = await _extractor.ScrapeAsync(url, ct);
            var cleaned = _extractor.CleanContent(blocks);

            if (cleaned.Count == 0)
                throw new InvalidOperationException("No usable content found on the page.");

            var full = string.Join("\n", cleaned);
            _chunks = _chunker.Chunk(full, _ragOptions.ChunkSize, _ragOptions.ChunkOverlap);

            Console.WriteLine($"Chunks created: {_chunks.Count}");
            Console.WriteLine($"Sample chunk: {_chunks[0][..Math.Min(200, _chunks[0].Length)]}...");

            Console.WriteLine("Creating embeddings...");
            var embeddings = await _openai.CreateEmbeddingsAsync(_chunks, ct);

            Console.WriteLine($"Embedding dims: {embeddings[0].Length}");
            Console.WriteLine("Sample embedding (first 10): " + string.Join(", ", embeddings[0].Take(10).Select(v => v.ToString("0.####"))));

            _store = new InMemoryVectorStoreService(_chunks, embeddings);
            _memory.Clear(); // reset memory when new site is loaded
        }

        public async Task<string> AskAsync(string question, CancellationToken ct)
        {
            if (_store is null || _chunks is null)
                throw new InvalidOperationException("Website not loaded. Call LoadWebsiteAsync first.");

            // Embed query
            var qVec = await _openai.CreateEmbeddingAsync(question, ct);

            // Similarity search
            var top = _store.TopK(qVec, _ragOptions.TopK);

            Console.WriteLine("\nTop relevant chunks:");
            var context = new StringBuilder();
            for (int i = 0; i < top.Count; i++)
            {
                Console.WriteLine($"{i + 1}. Similarity: {top[i].score:0.####}");
                Console.WriteLine($"   {top[i].text[..Math.Min(200, top[i].text.Length)]}...\n");
                context.AppendLine(top[i].text).AppendLine();
            }

            var historyBlock = _memory.ToTextBlock();

            var prompt = PROMPT_TEMPLATE
                .Replace("{context}", context.ToString())
                .Replace("{history}", historyBlock)
                .Replace("{question}", question);

            Console.WriteLine("\nFull prompt sent to model:\n" + prompt);
            Console.WriteLine("\n" + new string('=', 60) + "\n");

            var answer = await _openai.ChatAsync(prompt, _ragOptions.Temperature, _ragOptions.MaxTokens, ct);

            _memory.Add(question, answer);
            return answer;
        }
    }
}
