using System.Text;
using Lumindex.Application.Common.Ingestion;

namespace Lumindex.Infrastructure.Ingestion;

/// <summary>
/// Splits document pages into overlapping, token-bounded chunks. Text is broken on paragraph
/// boundaries and packed greedily up to <see cref="MaxTokens"/>; each new chunk repeats the trailing
/// ~<see cref="OverlapTokens"/> tokens of the previous one so context isn't lost across boundaries.
/// A chunk's page number is the page its first segment came from, giving citations a sensible anchor.
/// </summary>
public sealed class TokenAwareTextChunker : ITextChunker
{
    /// <summary>Target maximum tokens per chunk (~800, per the RAG design).</summary>
    private const int MaxTokens = 800;

    /// <summary>Tokens of overlap carried from the end of one chunk into the start of the next.</summary>
    private const int OverlapTokens = 100;

    private readonly ITokenCounter _tokenCounter;

    public TokenAwareTextChunker(ITokenCounter tokenCounter)
    {
        _tokenCounter = tokenCounter;
    }

    public IReadOnlyList<TextChunk> Chunk(IReadOnlyList<DocumentPage> pages)
    {
        var segments = BuildSegments(pages);
        if (segments.Count == 0)
        {
            return Array.Empty<TextChunk>();
        }

        var chunks = new List<TextChunk>();
        var current = new List<Segment>();
        var currentTokens = 0;
        var ordinal = 0;

        foreach (var segment in segments)
        {
            if (current.Count > 0 && currentTokens + segment.Tokens > MaxTokens)
            {
                chunks.Add(CreateChunk(ordinal++, current));

                var overlap = TakeOverlap(current);
                current = new List<Segment>(overlap);
                currentTokens = current.Sum(s => s.Tokens);
            }

            current.Add(segment);
            currentTokens += segment.Tokens;
        }

        if (current.Count > 0)
        {
            chunks.Add(CreateChunk(ordinal, current));
        }

        return chunks;
    }

    private List<Segment> BuildSegments(IReadOnlyList<DocumentPage> pages)
    {
        var segments = new List<Segment>();

        foreach (var page in pages)
        {
            foreach (var paragraph in SplitParagraphs(page.Text))
            {
                var tokens = _tokenCounter.CountTokens(paragraph);
                if (tokens <= MaxTokens)
                {
                    segments.Add(new Segment(page.PageNumber, paragraph, tokens));
                    continue;
                }

                // A single paragraph exceeds the budget: split it further by words so no chunk
                // ever overflows the model context window.
                foreach (var piece in SplitLargeText(paragraph))
                {
                    segments.Add(new Segment(page.PageNumber, piece, _tokenCounter.CountTokens(piece)));
                }
            }
        }

        return segments;
    }

    private TextChunk CreateChunk(int ordinal, List<Segment> segments)
    {
        var content = string.Join("\n\n", segments.Select(s => s.Text));
        var pageNumber = segments[0].PageNumber;
        return new TextChunk(ordinal, pageNumber, content, _tokenCounter.CountTokens(content));
    }

    private static List<Segment> TakeOverlap(List<Segment> segments)
    {
        var overlap = new List<Segment>();
        var tokens = 0;

        for (var i = segments.Count - 1; i >= 0; i--)
        {
            if (tokens >= OverlapTokens)
            {
                break;
            }

            overlap.Insert(0, segments[i]);
            tokens += segments[i].Tokens;
        }

        return overlap;
    }

    private static IEnumerable<string> SplitParagraphs(string text)
    {
        // Paragraphs are separated by one or more blank lines; fall back to single newlines.
        var blocks = text.Split("\n\n", StringSplitOptions.RemoveEmptyEntries);
        foreach (var block in blocks)
        {
            var trimmed = block.Trim();
            if (trimmed.Length > 0)
            {
                yield return trimmed;
            }
        }
    }

    private IEnumerable<string> SplitLargeText(string text)
    {
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var builder = new StringBuilder();
        var tokens = 0;

        foreach (var word in words)
        {
            var wordTokens = _tokenCounter.CountTokens(word);
            if (builder.Length > 0 && tokens + wordTokens > MaxTokens)
            {
                yield return builder.ToString();
                builder.Clear();
                tokens = 0;
            }

            if (builder.Length > 0)
            {
                builder.Append(' ');
            }

            builder.Append(word);
            tokens += wordTokens;
        }

        if (builder.Length > 0)
        {
            yield return builder.ToString();
        }
    }

    private readonly record struct Segment(int? PageNumber, string Text, int Tokens);
}
