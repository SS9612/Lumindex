namespace Lumindex.Application.Common.Ingestion;

/// <summary>
/// Extracts plain text (with page boundaries where available) from an uploaded document stream.
/// Implementations select the strategy from the file extension / content type (PDF, DOCX, TXT).
/// </summary>
public interface ITextExtractor
{
    /// <summary>Returns <c>true</c> when this extractor can handle the given file.</summary>
    bool CanExtract(string fileName, string contentType);

    /// <summary>
    /// Reads <paramref name="content"/> and returns the document's text split into pages. The stream
    /// is consumed fully; callers own its lifetime.
    /// </summary>
    Task<IReadOnlyList<DocumentPage>> ExtractAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);
}
