using Lumindex.Application.Common.Ingestion;
using Lumindex.Application.Common.Persistence;
using Lumindex.Application.Common.Storage;
using Lumindex.Application.Documents.Models;
using Lumindex.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Lumindex.Application.Documents.Commands.UploadDocument;

public sealed class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, DocumentDto>
{
    private readonly IDocumentRepository _documents;
    private readonly IBlobStorage _blobStorage;
    private readonly IDocumentIngestionQueue _ingestionQueue;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UploadDocumentCommandHandler> _logger;

    public UploadDocumentCommandHandler(
        IDocumentRepository documents,
        IBlobStorage blobStorage,
        IDocumentIngestionQueue ingestionQueue,
        IUnitOfWork unitOfWork,
        ILogger<UploadDocumentCommandHandler> logger)
    {
        _documents = documents;
        _blobStorage = blobStorage;
        _ingestionQueue = ingestionQueue;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<DocumentDto> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
    {
        var documentId = Guid.NewGuid();
        var extension = Path.GetExtension(request.FileName);

        // Owner-scoped, collision-free blob path: {ownerId}/{documentId}{ext}
        var blobPath = $"{request.OwnerId:D}/{documentId:D}{extension}";

        var sizeBytes = await _blobStorage.UploadAsync(blobPath, request.Content, request.ContentType, cancellationToken);

        var document = new Document
        {
            Id = documentId,
            OwnerId = request.OwnerId,
            FileName = request.FileName,
            ContentType = request.ContentType,
            SizeBytes = sizeBytes,
            BlobPath = blobPath,
            Status = DocumentStatus.Pending,
        };

        _documents.Add(document);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Kick off async ingestion (extract → chunk → embed → index) once the upload is committed.
        _ingestionQueue.Enqueue(request.OwnerId, documentId);

        _logger.LogInformation(
            "Stored document {DocumentId} ({SizeBytes} bytes) for owner {OwnerId}; queued for ingestion",
            documentId,
            sizeBytes,
            request.OwnerId);

        return DocumentDto.FromEntity(document);
    }
}
