using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Lumindex.Application.Common.Ingestion;
using UglyToad.PdfPig;

namespace Lumindex.Infrastructure.Ingestion;

/// <summary>
/// Extracts text from the file types Lumindex accepts. PDFs are read page-by-page with PdfPig (so
/// citations keep page numbers), DOCX via the Open XML SDK, and plain text directly. The extension is
/// authoritative, mirroring the upload validation rules.
/// </summary>
public sealed class TextExtractor : ITextExtractor
{
    public bool CanExtract(string fileName, string contentType) =>
        TryGetKind(fileName) is not null;

    public async Task<IReadOnlyList<DocumentPage>> ExtractAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var kind = TryGetKind(fileName)
            ?? throw new NotSupportedException($"Unsupported document type for '{fileName}'.");

        // PdfPig and the Open XML SDK need seekable streams; buffer once so any IBlobStorage works.
        await using var buffer = await BufferAsync(content, cancellationToken);

        return kind switch
        {
            DocumentKind.Pdf => ExtractPdf(buffer),
            DocumentKind.Docx => ExtractDocx(buffer),
            DocumentKind.Text => await ExtractTextAsync(buffer, cancellationToken),
            _ => throw new NotSupportedException($"Unsupported document type for '{fileName}'."),
        };
    }

    private static IReadOnlyList<DocumentPage> ExtractPdf(Stream stream)
    {
        var pages = new List<DocumentPage>();

        // Read the raw byte array so PdfPig always has a seekable buffer to work with.
        var bytes = ReadAllBytes(stream);
        using var pdf = PdfDocument.Open(bytes);
        foreach (var page in pdf.GetPages())
        {
            var text = page.Text;
            if (!string.IsNullOrWhiteSpace(text))
            {
                pages.Add(new DocumentPage(page.Number, Normalize(text)));
            }
        }

        return pages;
    }

    private static IReadOnlyList<DocumentPage> ExtractDocx(Stream stream)
    {
        using var word = WordprocessingDocument.Open(stream, isEditable: false);
        var body = word.MainDocumentPart?.Document.Body;
        if (body is null)
        {
            return Array.Empty<DocumentPage>();
        }

        var builder = new StringBuilder();
        foreach (var paragraph in body.Descendants<Paragraph>())
        {
            var text = paragraph.InnerText;
            if (!string.IsNullOrWhiteSpace(text))
            {
                builder.AppendLine(text);
            }
        }

        var content = Normalize(builder.ToString());
        return string.IsNullOrWhiteSpace(content)
            ? Array.Empty<DocumentPage>()
            // DOCX has no fixed pagination; page numbers are left null.
            : new[] { new DocumentPage(null, content) };
    }

    private static async Task<IReadOnlyList<DocumentPage>> ExtractTextAsync(Stream stream, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var content = Normalize(await reader.ReadToEndAsync(cancellationToken));
        return string.IsNullOrWhiteSpace(content)
            ? Array.Empty<DocumentPage>()
            : new[] { new DocumentPage(1, content) };
    }

    private static async Task<Stream> BufferAsync(Stream content, CancellationToken cancellationToken)
    {
        if (content is MemoryStream seekable && seekable.CanSeek)
        {
            seekable.Position = 0;
            return seekable;
        }

        var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);
        buffer.Position = 0;
        return buffer;
    }

    private static byte[] ReadAllBytes(Stream stream)
    {
        if (stream is MemoryStream memory)
        {
            return memory.ToArray();
        }

        using var buffer = new MemoryStream();
        stream.CopyTo(buffer);
        return buffer.ToArray();
    }

    private static string Normalize(string text) =>
        text.Replace("\r\n", "\n").Replace('\r', '\n').Trim();

    private static DocumentKind? TryGetKind(string fileName) =>
        Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".pdf" => DocumentKind.Pdf,
            ".docx" => DocumentKind.Docx,
            ".txt" => DocumentKind.Text,
            _ => null,
        };

    private enum DocumentKind
    {
        Pdf,
        Docx,
        Text,
    }
}
