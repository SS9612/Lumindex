using Lumindex.Application.Common.Ingestion;
using Hangfire;

namespace Lumindex.Infrastructure.BackgroundJobs;

/// <summary>
/// Hangfire-backed implementation of <see cref="IDocumentIngestionQueue"/>. Enqueues a fire-and-forget
/// job that resolves <see cref="IDocumentIngestionService"/> from DI and runs the pipeline. The
/// <see cref="CancellationToken"/> argument is a placeholder that Hangfire replaces with its shutdown
/// token at execution time.
/// </summary>
public sealed class HangfireDocumentIngestionQueue : IDocumentIngestionQueue
{
    private readonly IBackgroundJobClient _backgroundJobClient;

    public HangfireDocumentIngestionQueue(IBackgroundJobClient backgroundJobClient)
    {
        _backgroundJobClient = backgroundJobClient;
    }

    public void Enqueue(Guid ownerId, Guid documentId) =>
        _backgroundJobClient.Enqueue<IDocumentIngestionService>(
            service => service.IngestAsync(ownerId, documentId, CancellationToken.None));
}
