namespace Lumindex.Application.Common.Storage;

/// <summary>
/// Abstraction over the binary object store that holds uploaded document files. Implemented
/// over Azure Blob Storage in production and over the local file system for offline dev.
/// </summary>
public interface IBlobStorage
{
    /// <summary>
    /// Streams <paramref name="content"/> to the given <paramref name="blobPath"/> and returns the
    /// number of bytes written. Overwrites any existing blob at the same path.
    /// </summary>
    Task<long> UploadAsync(
        string blobPath,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default);

    /// <summary>Removes the blob at <paramref name="blobPath"/>; succeeds even when it does not exist.</summary>
    Task DeleteAsync(string blobPath, CancellationToken cancellationToken = default);

    /// <summary>Opens the blob at <paramref name="blobPath"/> for reading.</summary>
    Task<Stream> OpenReadAsync(string blobPath, CancellationToken cancellationToken = default);
}
