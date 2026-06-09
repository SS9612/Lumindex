namespace DocuMind.Application.Documents;

/// <summary>
/// Central definition of which files DocuMind accepts for ingestion. Shared between the API layer
/// (request size limits, multipart parsing) and the validation pipeline so the rules stay in sync.
/// </summary>
public static class DocumentUploadRules
{
    /// <summary>Maximum size of an individual uploaded file (25 MB).</summary>
    public const long MaxFileSizeBytes = 25L * 1024 * 1024;

    /// <summary>Maximum size of the whole multipart request, allowing headroom for boundaries/headers.</summary>
    public const long MaxRequestBytes = MaxFileSizeBytes + (1L * 1024 * 1024);

    public static readonly IReadOnlyCollection<string> AllowedExtensions = new[] { ".pdf", ".docx", ".txt" };

    public static readonly IReadOnlyCollection<string> AllowedContentTypes = new[]
    {
        "application/pdf",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "text/plain",
        // Browsers occasionally send a generic type (notably for .docx); the extension check is authoritative.
        "application/octet-stream",
    };

    public static string AllowedExtensionsDisplay => string.Join(", ", AllowedExtensions);

    public static bool HasAllowedExtension(string fileName) =>
        AllowedExtensions.Contains(Path.GetExtension(fileName), StringComparer.OrdinalIgnoreCase);

    public static bool HasAllowedContentType(string contentType) =>
        AllowedContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase);
}
