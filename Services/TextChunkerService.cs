namespace WebScrapeRag.Services
{
    sealed class TextChunkerService
    {
        public List<string> Chunk(string text, int chunkSize, int overlap)
        {
            if (chunkSize <= 0) throw new ArgumentOutOfRangeException(nameof(chunkSize));
            if (overlap < 0 || overlap >= chunkSize) throw new ArgumentOutOfRangeException(nameof(overlap));

            var chunks = new List<string>();
            int start = 0;

            while (start < text.Length)
            {
                int len = Math.Min(chunkSize, text.Length - start);
                var chunk = text.Substring(start, len).Trim();
                if (!string.IsNullOrWhiteSpace(chunk))
                    chunks.Add(chunk);

                if (start + len >= text.Length) break;
                start += (chunkSize - overlap);
            }

            return chunks;
        }
    }
}
