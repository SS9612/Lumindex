using Azure;
using Azure.AI.OpenAI;
using Lumindex.Application.Common.Ingestion;
using OpenAI.Embeddings;

namespace Lumindex.Infrastructure.Ingestion;

/// <summary>
/// Generates embeddings using an Azure OpenAI embedding deployment (e.g. <c>text-embedding-3-small</c>).
/// Inputs are batched to stay within the service's per-request item limit.
/// </summary>
public sealed class AzureOpenAIEmbeddingGenerator : IEmbeddingGenerator
{
    // Azure OpenAI accepts up to 2048 inputs per request; stay well under to keep payloads modest.
    private const int BatchSize = 96;

    private readonly EmbeddingClient _client;

    public AzureOpenAIEmbeddingGenerator(AzureOpenAIOptions options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(options.Endpoint);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.ApiKey);

        var azureClient = new AzureOpenAIClient(new Uri(options.Endpoint), new AzureKeyCredential(options.ApiKey));
        _client = azureClient.GetEmbeddingClient(options.EmbeddingDeployment);
        Dimensions = options.EmbeddingDimensions;
    }

    public int Dimensions { get; }

    public async Task<IReadOnlyList<ReadOnlyMemory<float>>> GenerateAsync(
        IReadOnlyList<string> inputs,
        CancellationToken cancellationToken = default)
    {
        if (inputs.Count == 0)
        {
            return Array.Empty<ReadOnlyMemory<float>>();
        }

        var options = new EmbeddingGenerationOptions { Dimensions = Dimensions };
        var results = new List<ReadOnlyMemory<float>>(inputs.Count);

        for (var offset = 0; offset < inputs.Count; offset += BatchSize)
        {
            var batch = inputs.Skip(offset).Take(BatchSize).ToList();
            var response = await _client.GenerateEmbeddingsAsync(batch, options, cancellationToken);

            foreach (var embedding in response.Value)
            {
                results.Add(embedding.ToFloats());
            }
        }

        return results;
    }
}
