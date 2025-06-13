using Microsoft.Extensions.Logging;

namespace Dzeta.TonWatcher.Core.Services;

public class BatchProcessor
{
    private readonly ITransactionRepository _repository;
    private readonly ILogger<BatchProcessor> _logger;
    private readonly int _batchSize;
    private int _currentBatchCount;

    public BatchProcessor(ITransactionRepository repository, ILogger<BatchProcessor> logger, int batchSize = 50)
    {
        _repository = repository;
        _logger = logger;
        _batchSize = batchSize;
        _currentBatchCount = 0;
    }

    public async Task ProcessItemAsync(CancellationToken cancellationToken = default)
    {
        _currentBatchCount++;
        
        if (_currentBatchCount >= _batchSize)
        {
            await SaveBatchAsync(cancellationToken);
        }
    }

    public async Task SaveBatchAsync(CancellationToken cancellationToken = default)
    {
        if (_currentBatchCount > 0)
        {
            await _repository.SaveChangesAsync(cancellationToken);
            _logger.LogDebug("Saved batch of {Count} transactions", _currentBatchCount);
            _currentBatchCount = 0;
        }
    }

    public async Task FinalizeAsync(CancellationToken cancellationToken = default)
    {
        await SaveBatchAsync(cancellationToken);
    }
} 