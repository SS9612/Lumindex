using Microsoft.Net.Http.Headers;

namespace Lumindex.Api.Common;

/// <summary>Helpers for parsing <c>multipart/form-data</c> uploads off the raw request stream.</summary>
public static class MultipartRequestHelper
{
    /// <summary>Extracts and validates the multipart boundary from the content type header.</summary>
    public static string GetBoundary(MediaTypeHeaderValue contentType, int lengthLimit)
    {
        var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary).Value;

        if (string.IsNullOrWhiteSpace(boundary))
        {
            throw new InvalidDataException("Missing content-type boundary.");
        }

        if (boundary.Length > lengthLimit)
        {
            throw new InvalidDataException($"Multipart boundary length limit {lengthLimit} exceeded.");
        }

        return boundary;
    }

    public static bool IsMultipartContentType(string? contentType) =>
        !string.IsNullOrEmpty(contentType) &&
        contentType.Contains("multipart/", StringComparison.OrdinalIgnoreCase);

    public static bool HasFileContentDisposition(ContentDispositionHeaderValue? contentDisposition) =>
        contentDisposition is not null &&
        contentDisposition.DispositionType.Equals("form-data") &&
        (!string.IsNullOrEmpty(contentDisposition.FileName.Value) ||
         !string.IsNullOrEmpty(contentDisposition.FileNameStar.Value));
}
