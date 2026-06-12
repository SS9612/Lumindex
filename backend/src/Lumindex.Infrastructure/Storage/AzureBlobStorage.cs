using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Lumindex.Application.Common.Storage;

namespace Lumindex.Infrastructure.Storage;

/// <summary>
/// <see cref="IBlobStorage"/> backed by Azure Blob Storage. Streams content straight through to the
/// configured container so large uploads are never fully buffered in memory.
/// </summary>
public sealed class AzureBlobStorage : IBlobStorage
{
    private readonly BlobContainerClient _container;

    public AzureBlobStorage(BlobServiceClient serviceClient, string containerName)
    {
        _container = serviceClient.GetBlobContainerClient(containerName);
    }

    public async Task<long> UploadAsync(
        string blobPath,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        await _container.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        var blob = _container.GetBlobClient(blobPath);
        await blob.UploadAsync(
            content,
            new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = contentType },
            },
            cancellationToken);

        var properties = await blob.GetPropertiesAsync(cancellationToken: cancellationToken);
        return properties.Value.ContentLength;
    }

    public Task DeleteAsync(string blobPath, CancellationToken cancellationToken = default) =>
        _container.GetBlobClient(blobPath).DeleteIfExistsAsync(cancellationToken: cancellationToken);

    public async Task<Stream> OpenReadAsync(string blobPath, CancellationToken cancellationToken = default) =>
        await _container.GetBlobClient(blobPath).OpenReadAsync(cancellationToken: cancellationToken);
}
