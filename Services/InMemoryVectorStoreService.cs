namespace WebScrapeRag.Services
{
    sealed class InMemoryVectorStoreService
    {
        private readonly List<string> _texts;
        private readonly List<float[]> _vectors;
        public InMemoryVectorStoreService(List<string> texts, List<float[]> vectors)
        {
            if (texts.Count != vectors.Count) throw new ArgumentException("Texts and vectors must match");
            _texts = texts;
            _vectors = vectors;
        }
        public List<(string text, double score)> TopK(float[] queryVector, int k)
        {
            var scored = new List<(string text, double score)>(_texts.Count);
            for (int i = 0; i < _vectors.Count; i++)
            {
                var score = CosineSimilarity(queryVector, _vectors[i]);
                scored.Add((_texts[i], score));
            }

            return scored.OrderByDescending(x => x.score).Take(k).ToList();
        }
        private static double CosineSimilarity(float[] a, float[] b)
        {
            if (a.Length != b.Length) throw new ArgumentException("Vector dims must match");

            double dot = 0, na = 0, nb = 0;
            for (int i = 0; i < a.Length; i++)
            {
                dot += a[i] * b[i];
                na += a[i] * a[i];
                nb += b[i] * b[i];
            }

            if (na == 0 || nb == 0) return 0;
            return dot / (Math.Sqrt(na) * Math.Sqrt(nb));
        }
    }
}
