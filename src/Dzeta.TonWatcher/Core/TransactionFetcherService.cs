using Dzeta.TonWatcher.Config;
using Dzeta.TonWatcher.Core.Services;
using Microsoft.Extensions.Logging;

namespace Dzeta.TonWatcher.Core;

/// <summary>
///     Orchestrates transaction fetching operations
/// </summary>
public class TransactionFetcherService
{
    private readonly LatestTransactionFetcher _latestFetcher;
    private readonly MissingTransactionFetcher _missingFetcher;
    private readonly ITransactionRepository _repository;
    private readonly TonWatcherConfiguration _config;
    private readonly ILogger<TransactionFetcherService> _logger;

    public TransactionFetcherService(
        LatestTransactionFetcher latestFetcher,
        MissingTransactionFetcher missingFetcher,
        ITransactionRepository repository,
        TonWatcherConfiguration config,
        ILogger<TransactionFetcherService> logger)
    {
        _latestFetcher = latestFetcher;
        _missingFetcher = missingFetcher;
        _repository = repository;
        _config = config;
        _logger = logger;
    }

    /// <summary>
    ///     Fetches latest transactions starting from the max LT in database
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of new transactions processed</returns>
    public async Task<int> FetchLatestTransactionsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _latestFetcher.FetchAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching latest transactions for wallet {WalletAddress}", _config.WalletAddress);
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
    public async Task<int> FetchMissingTransactionsAsync(long afterLt, long beforeLt, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _missingFetcher.FetchAsync(afterLt, beforeLt, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching missing transactions for wallet {WalletAddress}", _config.WalletAddress);
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
            var currentMaxLt = await _repository.GetMaxLtAsync(_config.WalletAddress, cancellationToken);

            if (currentMaxLt <= 0)
            {
                _logger.LogInformation("No transactions in database yet, skipping missing transaction fix");
                return 0;
            }

            return await FetchMissingTransactionsAsync(0, currentMaxLt, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during missing transaction fix for wallet {WalletAddress}", _config.WalletAddress);
            throw;
        }
    }
}