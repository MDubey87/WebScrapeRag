using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Embeddings;
using System.ClientModel;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;
using WebScrapeRag.Models;

namespace WebScrapeRag.Services
{
    sealed class OpenAiClientService
    {
        private readonly OpenAIClientOptions _openAiClientOptions;
        private readonly IChatClient _chatClient;
        private readonly GitHubModelOptions _ghModelOptions;
        private readonly string _token;
        private readonly string _chatModel;
        private readonly string _embeddingModel;
        public OpenAiClientService(IOptions<AppOptions> options)
        {
            _ghModelOptions = options.Value.GitHubModel;
            _token = _ghModelOptions.Token;
            if (string.IsNullOrWhiteSpace(_token))
                throw new ArgumentException("OpenAI API key is required in configuration");
            _chatModel = !string.IsNullOrWhiteSpace(_ghModelOptions.ChatModel) ? _ghModelOptions.ChatModel : "gpt-4o-mini";
            _embeddingModel = !string.IsNullOrWhiteSpace(_ghModelOptions.EmbeddingModel) ? _ghModelOptions.EmbeddingModel : "text-embedding-3-small";

            _openAiClientOptions = new OpenAIClientOptions()
            {
                Endpoint = new Uri("https://models.github.ai/inference")
            };
            var credentials = new ApiKeyCredential(_token);
            _chatClient = new OpenAIClient(credentials, _openAiClientOptions).GetChatClient(_chatModel).AsIChatClient();

        }

        public async Task<float[]> CreateEmbeddingAsync(string input, CancellationToken ct)
        {
            var list = await CreateEmbeddingsAsync(new List<string> { input }, ct);
            return list[0];
        }

        public async Task<List<float[]>> CreateEmbeddingsAsync(List<string> inputs, CancellationToken ct)
        {
            EmbeddingClient client = new(_embeddingModel, new ApiKeyCredential(_token), _openAiClientOptions);
            OpenAIEmbeddingCollection response = await client.GenerateEmbeddingsAsync(inputs);
            var result = new float[inputs.Count][];
            foreach (OpenAIEmbedding embedding in response)
            {
                ReadOnlyMemory<float> vector = embedding.ToFloats();
                int length = vector.Length;
                result[embedding.Index] = vector.ToArray();                
            }          

            return result.ToList();
        }

        public async Task<string> ChatAsync(string prompt, float temperature, int maxTokens, CancellationToken ct)
        {
            List<ChatMessage> chatMessages = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.System, "You are a helpful assistant."),
                new ChatMessage(ChatRole.User, prompt)
            };
            var chatResponse = await _chatClient.GetResponseAsync(chatMessages, new ChatOptions()
            {
                Temperature = temperature
            }, ct);
            return chatResponse.Text;
        }
    }
}
