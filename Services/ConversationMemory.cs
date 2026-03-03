namespace WebScrapeRag.Services
{
    sealed class ConversationMemory
    {
        private readonly List<(string user, string assistant)> _items = new();

        public void Add(string user, string assistant)
        {
            _items.Add((user, assistant));
            if (_items.Count > 20) _items.RemoveAt(0); // avoid infinite growth
        }

        public string ToTextBlock()
            => _items.Count == 0
                ? "(none)"
                : string.Join("\n", _items.Select(x => $"User: {x.user}\nAssistant: {x.assistant}\n"));

        public void Clear() => _items.Clear();
    }
}
