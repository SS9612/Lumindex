using Lumindex.Application.Documents.Models;
using MediatR;

namespace Lumindex.Application.Documents.Queries.ListDocuments;

/// <summary>Lists the owner's documents, newest first.</summary>
public sealed record ListDocumentsQuery(Guid OwnerId) : IRequest<IReadOnlyList<DocumentDto>>;
