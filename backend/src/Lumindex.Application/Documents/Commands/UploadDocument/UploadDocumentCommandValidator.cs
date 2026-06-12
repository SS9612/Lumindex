using FluentValidation;

namespace Lumindex.Application.Documents.Commands.UploadDocument;

public sealed class UploadDocumentCommandValidator : AbstractValidator<UploadDocumentCommand>
{
    public UploadDocumentCommandValidator()
    {
        RuleFor(x => x.OwnerId)
            .NotEmpty();

        RuleFor(x => x.FileName)
            .NotEmpty()
            .MaximumLength(512)
            .Must(DocumentUploadRules.HasAllowedExtension)
            .WithMessage($"Unsupported file type. Allowed extensions: {DocumentUploadRules.AllowedExtensionsDisplay}.");

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .MaximumLength(256)
            .Must(DocumentUploadRules.HasAllowedContentType)
            .WithMessage("Unsupported content type.");
    }
}
