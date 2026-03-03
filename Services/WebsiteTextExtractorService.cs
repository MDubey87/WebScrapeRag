using HtmlAgilityPack;

namespace WebScrapeRag.Services
{
    sealed class WebsiteTextExtractorService
    {
        private readonly HttpClient _httpClient;
        public WebsiteTextExtractorService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<List<string>> ScrapeAsync(string url, CancellationToken ct)
        {
            using var response = await _httpClient.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync(ct);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            // Remove script/style
            var removeNodes = doc.DocumentNode.SelectNodes("//script|//style");
            if (removeNodes != null)
                foreach (var n in removeNodes) n.Remove();
            var tags = new[] { "p", "h1", "h2", "h3", "h4", "h5", "h6", "li", "span", "div" };
            var results = new List<string>();
            foreach (var tag in tags)
            {
                var nodes = doc.DocumentNode.SelectNodes($"//{tag}");
                if (nodes == null) continue;
                foreach (var node in nodes)
                {
                    var text = HtmlEntity.DeEntitize(node.InnerText ?? "").Trim();
                    if (!string.IsNullOrWhiteSpace(text))
                        results.Add(NormalizeWhitespace(text));
                }
            }
            // Fallback body
            if (results.Count == 0)
            {
                var body = doc.DocumentNode.SelectSingleNode("//body");
                if (body != null)
                {
                    var text = HtmlEntity.DeEntitize(body.InnerText ?? "").Trim();
                    if (!string.IsNullOrWhiteSpace(text))
                        results.Add(NormalizeWhitespace(text));
                }
            }
            return results;
        }
        public List<string> CleanContent(List<string> contentList)
        {
            var blocked = new[] { "sign up", "sign in", "cookie", "privacy policy" };

            return contentList
                .Where(t => t.Length > 20)
                .Where(t => !blocked.Any(b => t.Contains(b, StringComparison.OrdinalIgnoreCase)))
                .Distinct()
                .ToList();
        }
        private static string NormalizeWhitespace(string input)
        => string.Join(' ', input.Split((char[])null!, StringSplitOptions.RemoveEmptyEntries));
    }
}
