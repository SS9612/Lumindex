using System.Security.Cryptography;
using System.Text;
using Lumindex.Application.Common.Ingestion;

namespace Lumindex.Infrastructure.Ingestion;

/// <summary>
/// Offline development/test fallback for <see cref="IEmbeddingGenerator"/>. It produces stable,
/// L2-normalized pseudo-vectors seeded from a hash of the input, so identical text always yields the
/// same vector. The vectors are not semantically meaningful — they exist purely so the full ingestion
/// pipeline (and the search index) can run end-to-end without provisioning Azure OpenAI.
/// </summary>
public sealed class DeterministicEmbeddingGenerator : IEmbeddingGenerator
{
    public DeterministicEmbeddingGenerator(int dimensions)
    {
        Dimensions = dimensions > 0 ? dimensions : 1536;
    }

    public int Dimensions { get; }

    public Task<IReadOnlyList<ReadOnlyMemory<float>>> GenerateAsync(
        IReadOnlyList<string> inputs,
        CancellationToken cancellationToken = default)
    {
        var results = new List<ReadOnlyMemory<float>>(inputs.Count);
        foreach (var input in inputs)
        {
            results.Add(Embed(input));
        }

        return Task.FromResult<IReadOnlyList<ReadOnlyMemory<float>>>(results);
    }

    private ReadOnlyMemory<float> Embed(string text)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(text ?? string.Empty));
        var seed = BitConverter.ToInt32(hash, 0);
        var random = new Random(seed);

        var vector = new float[Dimensions];
        double magnitude = 0;

        for (var i = 0; i < Dimensions; i++)
        {
            var value = (float)((random.NextDouble() * 2.0) - 1.0);
            vector[i] = value;
            magnitude += value * value;
        }

        magnitude = Math.Sqrt(magnitude);
        if (magnitude > 0)
        {
            for (var i = 0; i < Dimensions; i++)
            {
                vector[i] = (float)(vector[i] / magnitude);
            }
        }

        return vector;
    }
}
