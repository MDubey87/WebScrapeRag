# WebScrapeRag

Enhanced Web Scraping + RAG (Retrieval-Augmented Generation) sample using .NET Console and GitHub AI-compatible models.

## Overview

`WebScrapeRag` is a small .NET console application that demonstrates a RAG pipeline:

- Fetch and extract text content from a website
- Clean and chunk documents into overlapping passages
- Create embeddings for chunks using a GitHub/OpenAI-compatible embedding model
- Store vectors in memory and perform similarity search (Top-K)
- Compose a prompt combining context, conversation history and the user question
- Query a chat model and maintain short-term conversation memory

This project is intended as a minimal, opinionated example for experimenting with retrieval-augmented generation on web content.

## Tech stack

- .NET 10 (C# 14)
- Console application with `IHostedService`
- Uses GitHub Models (OpenAI-compatible) via `OpenAI` client

## Project structure (high level)

- `Program.cs` - Host and DI configuration
- `ConsoleApp.cs` - Interactive console host (load site, ask questions)
- `Services/WebsiteTextExtractorService.cs` - Fetch and extract page text
- `Services/TextChunkerService.cs` - Chunking logic
- `Services/InMemoryVectorStoreService.cs` - Vector store and Top-K search
- `Services/OpenAiClientService.cs` - Embeddings + chat client wrapper
- `Services/RagPipeline.cs` - Orchestrates the end-to-end pipeline
- `Services/ConversationMemory.cs` - Simple chat history memory
- `Models/AppOptions.cs`, `Models/RagOptions.cs` - Configuration models
- `appsettings.json` - Example config for models and RAG tuning

## Configuration

`appsettings.json` contains two sections used by the app:

- `GitHubModel` (rename / reconfigure to match your provider):
  - `Token` - API key for the model endpoint (do not commit secrets). Use environment variables for production.
  - `ChatModel` - Chat model id (e.g. `openai/gpt-4.1-mini` or provider-specific id)
  - `EmbeddingModel` - Embedding model id (e.g. `openai/text-embedding-3-small`)

- `Rag` - RAG pipeline tuning options:
  - `ChunkSize` (default 300)
  - `ChunkOverlap` (default 50)
  - `TopK` (default 3) — number of similar chunks to include as context
  - `Temperature` (default 0.4)
  - `MaxTokens` (default 1200)

Important: The repository's `appsettings.json` may contain an example token. Replace it with a safe value or use environment variables.

Example (do not use real keys in public repos):

```
{
  "GitHubModel": {
    "Token": "YOUR_API_KEY_HERE",
    "ChatModel": "openai/gpt-4.1-mini",
    "EmbeddingModel": "openai/text-embedding-3-small"
  },
  "Rag": {
    "ChunkSize": 300,
    "ChunkOverlap": 50,
    "TopK": 3,
    "Temperature": 0.4,
    "MaxTokens": 1200
  }
}
```

## Running the app

1. Install .NET 10 SDK.
2. Restore and build:

   `dotnet restore`
   `dotnet build`

3. Run from the project folder:

   `dotnet run --project <path-to-project-csproj>`

4. The console app will prompt:
   - Enter a website URL (e.g. `https://example.com`)
   - Wait while the site is scraped, chunked and embedded
   - Ask queries against the loaded website
   - Type `new` to load another site, `quit` to exit

Notes:
- Some sites rely on JavaScript and won't be fully scraped by a plain `HttpClient` request; consider using a headless browser for complex pages.
- Large pages may produce many chunks; adjust `ChunkSize` and `ChunkOverlap` to control memory and context size.

## Security and secrets

- Do NOT commit API keys or secrets to the repository. Use environment variables or protected configuration in real deployments.
- The sample `appsettings.json` should be used for local testing only and scrubbed before publishing.

## Extending the project

- Persist the vector store (SQLite, Faiss, Milvus) instead of in-memory storage for larger datasets
- Add HTML rendering / headless browser scraping for JavaScript-heavy pages
- Add concurrency safeguards and request throttling when scraping multiple sites
- Integrate a more robust prompt/template system and richer system messages

## Troubleshooting

- Error: "No usable content found on the page." — the extractor failed to find text; try a different URL or improve the extractor.
- Error about API key — check `GitHubModel:Token` or set the environment variable used by your hosting configuration.

## License

This project is provided as an example. No license specified.

## Contact / Source

Repository origin: https://github.com/MDubey87/WebScrapeRag
