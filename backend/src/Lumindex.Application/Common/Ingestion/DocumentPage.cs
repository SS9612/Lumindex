namespace Lumindex.Application.Common.Ingestion;

/// <summary>
/// A unit of text extracted from a source document, tagged with its 1-based source page number.
/// <see cref="PageNumber"/> is <c>null</c> for page-less formats (e.g. DOCX) so downstream citations
/// can fall back to ordinal-based references.
/// </summary>
public sealed record DocumentPage(int? PageNumber, string Text);
