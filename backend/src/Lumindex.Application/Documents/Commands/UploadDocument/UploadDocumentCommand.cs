using Lumindex.Application.Documents.Models;
using MediatR;

namespace Lumindex.Application.Documents.Commands.UploadDocument;

/// <summary>
/// Persists an uploaded file: streams <see cref="Content"/> to blob storage and records the document
/// metadata for the owner. The handler does not buffer the stream, so callers should pass the live
/// request/multipart section stream.
/// </summary>
public sealed record UploadDocumentCommand(
    Guid OwnerId,
    string FileName,
    string ContentType,
    Stream Content) : IRequest<DocumentDto>;
