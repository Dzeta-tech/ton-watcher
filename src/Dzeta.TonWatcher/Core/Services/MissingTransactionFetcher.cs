using Dzeta.TonWatcher.Config;
using Dzeta.TonWatcher.Generated;
using Dzeta.TonWatcher.Providers;
using Microsoft.Extensions.Logging;

namespace Dzeta.TonWatcher.Core.Services;

public class MissingTransactionFetcher
{
    private readonly TonApiService _tonApiService;
    private readonly ITransactionRepository _repository;
    private readonly TonWatcherConfiguration _config;
    private readonly ILogger<MissingTransactionFetcher> _logger;
    private readonly ILoggerFactory _loggerFactory;

    public MissingTransactionFetcher(
        TonApiService tonApiService,
        ITransactionRepository repository,
        TonWatcherConfiguration config,
        ILogger<MissingTransactionFetcher> logger,
        ILoggerFactory loggerFactory)
    {
        _tonApiService = tonApiService;
        _repository = repository;
        _config = config;
        _logger = logger;
        _loggerFactory = loggerFactory;
    }

    public async Task<int> FetchAsync(long afterLt, long beforeLt, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Starting missing transaction fetch from LT {AfterLt} to {BeforeLt} for wallet {WalletAddress}",
            afterLt, beforeLt, _config.WalletAddress);

        var processor = new BatchProcessor(_repository, _loggerFactory.CreateLogger<BatchProcessor>());
        var newTransactionCount = 0;
        var updatedTransactionCount = 0;

        await foreach (var apiTransaction in _tonApiService.GetTransactionsAfterLtAsync(
                           _config.WalletAddress, afterLt, beforeLt, cancellationToken: cancellationToken))
        {
            var existingTransaction = await _repository.GetByHashAsync(apiTransaction.Hash, cancellationToken);

            if (existingTransaction != null)
            {
                if (ShouldUpdateTransaction(existingTransaction, apiTransaction))
                {
                    TransactionMapper.UpdateDbTransaction(existingTransaction, apiTransaction);
                    await _repository.UpdateAsync(existingTransaction, cancellationToken);
                    await processor.ProcessItemAsync(cancellationToken);
                    
                    updatedTransactionCount++;
                    _logger.LogDebug("Updated transaction {Hash} from unsuccessful to successful", apiTransaction.Hash);
                }
                else
                {
                    _logger.LogDebug("Transaction {Hash} already exists and no update needed", apiTransaction.Hash);
                }
                continue;
            }

            var dbTransaction = TransactionMapper.ToDbTransaction(apiTransaction, _config.WalletAddress);
            await _repository.AddAsync(dbTransaction, cancellationToken);
            await processor.ProcessItemAsync(cancellationToken);
            
            newTransactionCount++;
        }

        await processor.FinalizeAsync(cancellationToken);

        _logger.LogInformation(
            "Processed {NewCount} new and {UpdatedCount} updated transactions for wallet {WalletAddress}",
            newTransactionCount, updatedTransactionCount, _config.WalletAddress);

        return newTransactionCount + updatedTransactionCount;
    }

    private static bool ShouldUpdateTransaction(Infrastructure.Entities.Transaction existingTransaction, Transaction apiTransaction)
    {
        return !existingTransaction.Success && apiTransaction.Success;
    }
} 