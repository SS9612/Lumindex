using Lumindex.Application.Common.Ingestion;

namespace Lumindex.Infrastructure.Ingestion;

/// <summary>
/// Lightweight, dependency-free token estimator. It blends a characters-per-token ratio with a
/// word count so both prose and dense/code-like text get a reasonable estimate without bundling a
/// tokenizer model. Swap for <c>Microsoft.ML.Tokenizers</c> (tiktoken) when exact counts matter.
/// </summary>
public sealed class HeuristicTokenCounter : ITokenCounter
{
    // Empirically ~4 characters per token for English text with the cl100k/o200k vocabularies.
    private const double CharactersPerToken = 4.0;

    public int CountTokens(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return 0;
        }

        var charEstimate = (int)Math.Ceiling(text.Length / CharactersPerToken);
        var wordEstimate = CountWords(text);

        // Take the larger estimate so we never under-count and overflow the model's context window.
        return Math.Max(1, Math.Max(charEstimate, wordEstimate));
    }

    private static int CountWords(string text)
    {
        var count = 0;
        var inWord = false;

        foreach (var ch in text)
        {
            if (char.IsWhiteSpace(ch))
            {
                inWord = false;
            }
            else if (!inWord)
            {
                inWord = true;
                count++;
            }
        }

        return count;
    }
}
