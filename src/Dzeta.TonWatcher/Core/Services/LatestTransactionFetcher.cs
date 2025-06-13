using Dzeta.TonWatcher.Config;
using Dzeta.TonWatcher.Generated;
using Dzeta.TonWatcher.Providers;
using Microsoft.Extensions.Logging;

namespace Dzeta.TonWatcher.Core.Services;

public class LatestTransactionFetcher
{
    private readonly TonApiService _tonApiService;
    private readonly ITransactionRepository _repository;
    private readonly TonWatcherConfiguration _config;
    private readonly ILogger<LatestTransactionFetcher> _logger;
    private readonly ILoggerFactory _loggerFactory;

    public LatestTransactionFetcher(
        TonApiService tonApiService,
        ITransactionRepository repository,
        TonWatcherConfiguration config,
        ILogger<LatestTransactionFetcher> logger,
        ILoggerFactory loggerFactory)
    {
        _tonApiService = tonApiService;
        _repository = repository;
        _config = config;
        _logger = logger;
        _loggerFactory = loggerFactory;
    }

    public async Task<int> FetchAsync(CancellationToken cancellationToken = default)
    {
        var maxLt = await _repository.GetMaxLtAsync(_config.WalletAddress, cancellationToken);
        
        _logger.LogInformation("Starting latest transaction fetch from LT {MaxLt} for wallet {WalletAddress}",
            maxLt, _config.WalletAddress);

        var processor = new BatchProcessor(_repository, _loggerFactory.CreateLogger<BatchProcessor>());
        var newTransactionCount = 0;

        await foreach (var apiTransaction in _tonApiService.GetTransactionsAfterLtAsync(
                           _config.WalletAddress, maxLt, beforeLt: 0, cancellationToken: cancellationToken))
        {
            var existingTransaction = await _repository.GetByHashAsync(apiTransaction.Hash, cancellationToken);
            if (existingTransaction != null)
            {
                _logger.LogDebug("Transaction {Hash} already exists, skipping", apiTransaction.Hash);
                continue;
            }

            var dbTransaction = TransactionMapper.ToDbTransaction(apiTransaction, _config.WalletAddress);
            await _repository.AddAsync(dbTransaction, cancellationToken);
            await processor.ProcessItemAsync(cancellationToken);
            
            newTransactionCount++;
            _logger.LogDebug("Added new transaction {Hash} with LT {Lt}", dbTransaction.Hash, dbTransaction.Lt);
        }

        await processor.FinalizeAsync(cancellationToken);
        
        _logger.LogInformation("Processed {Count} new transactions for wallet {WalletAddress}",
            newTransactionCount, _config.WalletAddress);

        return newTransactionCount;
    }
} 