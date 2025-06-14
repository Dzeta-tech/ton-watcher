using Dzeta.TonWatcher.Config;
using Dzeta.TonWatcher.Infrastructure.Entities;
using Dzeta.TonWatcher.Providers;
using Microsoft.Extensions.Logging;

namespace Dzeta.TonWatcher.Core.Services;

public class MissingTransactionFetcher(
    TonApiService tonApiService,
    ITransactionRepository repository,
    TonWatcherConfiguration config,
    ILogger<MissingTransactionFetcher> logger,
    ILoggerFactory loggerFactory)
{
    public async Task<int> FetchAsync(long afterLt, long beforeLt, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Starting missing transaction fetch from LT {AfterLt} to {BeforeLt} for wallet {WalletAddress}",
            afterLt, beforeLt, config.WalletAddress);

        BatchProcessor processor = new(repository, loggerFactory.CreateLogger<BatchProcessor>());
        int newTransactionCount = 0;
        int updatedTransactionCount = 0;

        await foreach (Generated.Transaction apiTransaction in tonApiService.GetTransactionsAfterLtAsync(
                           config.WalletAddress, afterLt, beforeLt, cancellationToken: cancellationToken))
        {
            Transaction? existingTransaction = await repository.GetByHashAsync(apiTransaction.Hash, cancellationToken);

            if (existingTransaction != null)
            {
                if (ShouldUpdateTransaction(existingTransaction, apiTransaction))
                {
                    TransactionMapper.UpdateDbTransaction(existingTransaction, apiTransaction);
                    await repository.UpdateAsync(existingTransaction, cancellationToken);
                    await processor.ProcessItemAsync(cancellationToken);

                    updatedTransactionCount++;
                    logger.LogDebug("Updated transaction {Hash} from unsuccessful to successful", apiTransaction.Hash);
                }
                else
                {
                    logger.LogDebug("Transaction {Hash} already exists and no update needed", apiTransaction.Hash);
                }

                continue;
            }

            Transaction dbTransaction = TransactionMapper.ToDbTransaction(apiTransaction, config.WalletAddress);
            await repository.AddAsync(dbTransaction, cancellationToken);
            await processor.ProcessItemAsync(cancellationToken);

            newTransactionCount++;
        }

        await processor.FinalizeAsync(cancellationToken);

        logger.LogInformation(
            "Processed {NewCount} new and {UpdatedCount} updated transactions for wallet {WalletAddress}",
            newTransactionCount, updatedTransactionCount, config.WalletAddress);

        return newTransactionCount + updatedTransactionCount;
    }

    static bool ShouldUpdateTransaction(Transaction existingTransaction, Generated.Transaction apiTransaction)
    {
        return !existingTransaction.Success && apiTransaction.Success;
    }
}