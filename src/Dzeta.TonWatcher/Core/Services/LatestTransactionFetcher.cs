using Dzeta.TonWatcher.Config;
using Dzeta.TonWatcher.Generated;
using Dzeta.TonWatcher.Providers;
using Microsoft.Extensions.Logging;

namespace Dzeta.TonWatcher.Core.Services;

public class LatestTransactionFetcher(
    TonApiService tonApiService,
    ITransactionRepository repository,
    TonWatcherConfiguration config,
    ILogger<LatestTransactionFetcher> logger,
    ILoggerFactory loggerFactory)
{
    public async Task<int> FetchAsync(CancellationToken cancellationToken = default)
    {
        long maxLt = await repository.GetMaxLtAsync(config.WalletAddress, cancellationToken);

        logger.LogInformation("Starting latest transaction fetch from LT {MaxLt} for wallet {WalletAddress}",
            maxLt, config.WalletAddress);

        BatchProcessor processor = new(repository, loggerFactory.CreateLogger<BatchProcessor>());
        int newTransactionCount = 0;

        await foreach (Transaction apiTransaction in tonApiService.GetTransactionsAfterLtAsync(
                           config.WalletAddress, maxLt, 0, cancellationToken: cancellationToken))
        {
            Infrastructure.Entities.Transaction? existingTransaction =
                await repository.GetByHashAsync(apiTransaction.Hash, cancellationToken);
            if (existingTransaction != null)
            {
                logger.LogDebug("Transaction {Hash} already exists, skipping", apiTransaction.Hash);
                continue;
            }

            Infrastructure.Entities.Transaction dbTransaction =
                TransactionMapper.ToDbTransaction(apiTransaction, config.WalletAddress);
            await repository.AddAsync(dbTransaction, cancellationToken);
            await processor.ProcessItemAsync(cancellationToken);

            newTransactionCount++;
            logger.LogDebug("Added new transaction {Hash} with LT {Lt}", dbTransaction.Hash, dbTransaction.Lt);
        }

        await processor.FinalizeAsync(cancellationToken);

        logger.LogInformation("Processed {Count} new transactions for wallet {WalletAddress}",
            newTransactionCount, config.WalletAddress);

        return newTransactionCount;
    }
}