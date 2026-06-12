using Lumindex.Application.Documents.Models;
using MediatR;

namespace Lumindex.Application.Documents.Queries.GetDocument;

/// <summary>Returns a single owner-scoped document, or <c>null</c> when it does not exist.</summary>
public sealed record GetDocumentQuery(Guid OwnerId, Guid DocumentId) : IRequest<DocumentDto?>;
