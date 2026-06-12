using Lumindex.Application.Common.Persistence;
using Lumindex.Application.Documents.Models;
using MediatR;

namespace Lumindex.Application.Documents.Queries.ListDocuments;

public sealed class ListDocumentsQueryHandler : IRequestHandler<ListDocumentsQuery, IReadOnlyList<DocumentDto>>
{
    private readonly IDocumentRepository _documents;

    public ListDocumentsQueryHandler(IDocumentRepository documents)
    {
        _documents = documents;
    }

    public async Task<IReadOnlyList<DocumentDto>> Handle(ListDocumentsQuery request, CancellationToken cancellationToken)
    {
        var documents = await _documents.ListAsync(request.OwnerId, cancellationToken);
        return documents.Select(DocumentDto.FromEntity).ToList();
    }
}
