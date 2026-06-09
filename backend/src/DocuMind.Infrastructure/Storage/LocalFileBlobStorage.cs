using DocuMind.Application.Common.Storage;

namespace DocuMind.Infrastructure.Storage;

/// <summary>
/// File-system implementation of <see cref="IBlobStorage"/> used for local development when no Azure
/// Storage connection string is configured. Blob paths map to a directory tree under the root folder.
/// </summary>
public sealed class LocalFileBlobStorage : IBlobStorage
{
    private readonly string _root;

    public LocalFileBlobStorage(string rootPath)
    {
        _root = Path.GetFullPath(rootPath);
        Directory.CreateDirectory(_root);
    }

    public async Task<long> UploadAsync(
        string blobPath,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var fullPath = ResolvePath(blobPath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        await using var file = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await content.CopyToAsync(file, cancellationToken);
        return file.Length;
    }

    public Task DeleteAsync(string blobPath, CancellationToken cancellationToken = default)
    {
        var fullPath = ResolvePath(blobPath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    public Task<Stream> OpenReadAsync(string blobPath, CancellationToken cancellationToken = default)
    {
        var fullPath = ResolvePath(blobPath);
        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult(stream);
    }

    private string ResolvePath(string blobPath)
    {
        var normalized = blobPath
            .Replace('\\', Path.DirectorySeparatorChar)
            .Replace('/', Path.DirectorySeparatorChar);

        var combined = Path.GetFullPath(Path.Combine(_root, normalized));

        // Guard against path traversal (e.g. "../") escaping the storage root.
        if (!combined.StartsWith(_root, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Resolved blob path escapes the storage root.");
        }

        return combined;
    }
}
