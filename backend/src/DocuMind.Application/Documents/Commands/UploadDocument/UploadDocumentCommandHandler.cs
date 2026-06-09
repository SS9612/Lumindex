using DocuMind.Application.Common.Persistence;
using DocuMind.Application.Common.Storage;
using DocuMind.Application.Documents.Models;
using DocuMind.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DocuMind.Application.Documents.Commands.UploadDocument;

public sealed class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, DocumentDto>
{
    private readonly IDocumentRepository _documents;
    private readonly IBlobStorage _blobStorage;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UploadDocumentCommandHandler> _logger;

    public UploadDocumentCommandHandler(
        IDocumentRepository documents,
        IBlobStorage blobStorage,
        IUnitOfWork unitOfWork,
        ILogger<UploadDocumentCommandHandler> logger)
    {
        _documents = documents;
        _blobStorage = blobStorage;
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

        _logger.LogInformation(
            "Stored document {DocumentId} ({SizeBytes} bytes) for owner {OwnerId}",
            documentId,
            sizeBytes,
            request.OwnerId);

        return DocumentDto.FromEntity(document);
    }
}
