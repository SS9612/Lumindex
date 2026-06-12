using Lumindex.Api.Common;
using Lumindex.Api.Contracts.Documents;
using Lumindex.Application.Common.Interfaces;
using Lumindex.Application.Documents;
using Lumindex.Application.Documents.Commands.UploadDocument;
using Lumindex.Application.Documents.Models;
using Lumindex.Application.Documents.Queries.GetDocument;
using Lumindex.Application.Documents.Queries.ListDocuments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace Lumindex.Api.Controllers;

[ApiController]
[Route("api/documents")]
[Authorize]
[Produces("application/json")]
public sealed class DocumentsController : ControllerBase
{
    // Multipart boundaries are short; align with the framework default form option.
    private const int BoundaryLengthLimit = 70;

    private readonly ISender _mediator;
    private readonly ICurrentUser _currentUser;

    public DocumentsController(ISender mediator, ICurrentUser currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    /// <summary>List the current user's documents, newest first.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DocumentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        if (_currentUser.Id is not { } userId)
        {
            return Unauthorized();
        }

        var documents = await _mediator.Send(new ListDocumentsQuery(userId), cancellationToken);
        return Ok(documents.Select(ToResponse));
    }

    /// <summary>Return a single document owned by the current user.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DocumentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        if (_currentUser.Id is not { } userId)
        {
            return Unauthorized();
        }

        var document = await _mediator.Send(new GetDocumentQuery(userId, id), cancellationToken);
        return document is null ? NotFound() : Ok(ToResponse(document));
    }

    /// <summary>Upload a document. The file is streamed straight to blob storage without buffering.</summary>
    [HttpPost]
    [DisableFormValueModelBinding]
    [RequestSizeLimit(DocumentUploadRules.MaxRequestBytes)]
    [ProducesResponseType(typeof(DocumentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
    public async Task<IActionResult> Upload(CancellationToken cancellationToken)
    {
        if (_currentUser.Id is not { } userId)
        {
            return Unauthorized();
        }

        if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
        {
            return Problem(
                detail: "Expected a multipart/form-data request.",
                statusCode: StatusCodes.Status415UnsupportedMediaType,
                title: "Unsupported media type");
        }

        var boundary = MultipartRequestHelper.GetBoundary(
            MediaTypeHeaderValue.Parse(Request.ContentType),
            BoundaryLengthLimit);

        var reader = new MultipartReader(boundary, Request.Body);
        var section = await reader.ReadNextSectionAsync(cancellationToken);

        while (section is not null)
        {
            var hasContentDisposition = ContentDispositionHeaderValue.TryParse(
                section.ContentDisposition,
                out var contentDisposition);

            if (hasContentDisposition && MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
            {
                var rawFileName = contentDisposition!.FileNameStar.Value ?? contentDisposition.FileName.Value;
                var fileName = Path.GetFileName(HeaderUtilities.RemoveQuotes(rawFileName).Value ?? string.Empty);
                var contentType = section.ContentType ?? "application/octet-stream";

                // Validation runs in the MediatR pipeline before the stream is consumed, so
                // unsupported files are rejected without ever touching blob storage.
                var document = await _mediator.Send(
                    new UploadDocumentCommand(userId, fileName, contentType, section.Body),
                    cancellationToken);

                return CreatedAtAction(nameof(Get), new { id = document.Id }, ToResponse(document));
            }

            section = await reader.ReadNextSectionAsync(cancellationToken);
        }

        return Problem(
            detail: "No file was provided in the request.",
            statusCode: StatusCodes.Status400BadRequest,
            title: "Missing file");
    }

    private static DocumentResponse ToResponse(DocumentDto document) => new(
        document.Id,
        document.FileName,
        document.ContentType,
        document.SizeBytes,
        document.Status,
        document.StatusDetail,
        document.ChunkCount,
        document.CreatedAt,
        document.ProcessedAt);
}
