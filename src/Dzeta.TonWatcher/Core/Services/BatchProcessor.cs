using Microsoft.Extensions.Logging;

namespace Dzeta.TonWatcher.Core.Services;

public class BatchProcessor(ITransactionRepository repository, ILogger<BatchProcessor> logger, int batchSize = 50)
{
    int currentBatchCount;

    public async Task ProcessItemAsync(CancellationToken cancellationToken = default)
    {
        currentBatchCount++;

        if (currentBatchCount >= batchSize) await SaveBatchAsync(cancellationToken);
    }

    public async Task SaveBatchAsync(CancellationToken cancellationToken = default)
    {
        if (currentBatchCount > 0)
        {
            await repository.SaveChangesAsync(cancellationToken);
            logger.LogDebug("Saved batch of {Count} transactions", currentBatchCount);
            currentBatchCount = 0;
        }
    }

    public async Task FinalizeAsync(CancellationToken cancellationToken = default)
    {
        await SaveBatchAsync(cancellationToken);
    }
}