using Dzeta.TonWatcher.Config;
using Dzeta.TonWatcher.Core.Services;
using Microsoft.Extensions.Logging;

namespace Dzeta.TonWatcher.Core;

/// <summary>
///     Orchestrates transaction fetching operations
/// </summary>
public class TransactionFetcherService(
    LatestTransactionFetcher latestFetcher,
    MissingTransactionFetcher missingFetcher,
    ITransactionRepository repository,
    TonWatcherConfiguration config,
    ILogger<TransactionFetcherService> logger)
{
    /// <summary>
    ///     Fetches latest transactions starting from the max LT in database
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of new transactions processed</returns>
    public async Task<int> FetchLatestTransactionsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await latestFetcher.FetchAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching latest transactions for wallet {WalletAddress}",
                config.WalletAddress);
            throw;
        }
    }

    /// <summary>
    ///     Fetches transactions in a specific range to fix missing transactions
    /// </summary>
    /// <param name="afterLt">Start LT (not inclusive)</param>
    /// <param name="beforeLt">End LT to avoid race conditions with latest fetch</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of transactions processed</returns>
    public async Task<int> FetchMissingTransactionsAsync(long afterLt, long beforeLt,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await missingFetcher.FetchAsync(afterLt, beforeLt, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching missing transactions for wallet {WalletAddress}",
                config.WalletAddress);
            throw;
        }
    }

    /// <summary>
    ///     Job method for fixing missing transactions with automatic LT range detection
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of transactions processed</returns>
    public async Task<int> FixMissingTransactionsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            long currentMaxLt = await repository.GetMaxLtAsync(config.WalletAddress, cancellationToken);

            if (currentMaxLt <= 0)
            {
                logger.LogInformation("No transactions in database yet, skipping missing transaction fix");
                return 0;
            }

            return await FetchMissingTransactionsAsync(0, currentMaxLt, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during missing transaction fix for wallet {WalletAddress}",
                config.WalletAddress);
            throw;
        }
    }
}