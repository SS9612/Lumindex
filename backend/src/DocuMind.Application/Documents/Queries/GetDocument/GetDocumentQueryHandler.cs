using DocuMind.Application.Common.Persistence;
using DocuMind.Application.Documents.Models;
using MediatR;

namespace DocuMind.Application.Documents.Queries.GetDocument;

public sealed class GetDocumentQueryHandler : IRequestHandler<GetDocumentQuery, DocumentDto?>
{
    private readonly IDocumentRepository _documents;

    public GetDocumentQueryHandler(IDocumentRepository documents)
    {
        _documents = documents;
    }

    public async Task<DocumentDto?> Handle(GetDocumentQuery request, CancellationToken cancellationToken)
    {
        var document = await _documents.GetByIdAsync(request.OwnerId, request.DocumentId, cancellationToken);
        return document is null ? null : DocumentDto.FromEntity(document);
    }
}
