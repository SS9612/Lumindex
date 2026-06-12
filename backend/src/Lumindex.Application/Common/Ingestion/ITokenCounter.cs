namespace Lumindex.Application.Common.Ingestion;

/// <summary>
/// Estimates the number of model tokens a piece of text consumes. Abstracted so the chunker stays
/// independent of any specific tokenizer and can be swapped for an exact tiktoken implementation.
/// </summary>
public interface ITokenCounter
{
    int CountTokens(string text);
}
