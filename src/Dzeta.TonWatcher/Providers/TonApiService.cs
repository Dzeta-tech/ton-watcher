using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.RateLimiting;
using Dzeta.TonWatcher.Generated;
using Microsoft.Extensions.Logging;

namespace Dzeta.TonWatcher.Providers;

/// <summary>
///     Service for fetching TON transactions with rate limiting and pagination
/// </summary>
public class TonApiService(TonApiClient tonApiClient, ILogger<TonApiService> logger) : IDisposable
{
    readonly RateLimiter rateLimiter = new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions
    {
        PermitLimit = 1,
        QueueLimit = 0,
        Window = TimeSpan.FromSeconds(1)
    });

    /// <summary>
    ///     Gets all transactions for account after specific LT with beforeLt limit
    /// </summary>
    /// <param name="accountAddress">TON wallet address</param>
    /// <param name="afterLt">Return only transactions after this LT (NOT inclusive)</param>
    /// <param name="beforeLt">Return only transactions before this LT</param>
    /// <param name="limit">Number of transactions per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Async enumerable of transactions</returns>
    public async IAsyncEnumerable<Transaction> GetTransactionsAfterLtAsync(
        string accountAddress,
        long afterLt,
        long beforeLt,
        int limit = 250,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        bool hasMore = true;
        long currentAfterLt = afterLt;

        while (hasMore && !cancellationToken.IsCancellationRequested)
        {
            using RateLimitLease? lease = await rateLimiter.AcquireAsync(1, cancellationToken);
            if (!lease.IsAcquired)
            {
                logger.LogWarning("Rate limit exceeded, skipping request");
                yield break;
            }

            logger.LogDebug(
                "Fetching transactions for {AccountAddress} after LT {AfterLt} before LT {BeforeLt} with limit {Limit}",
                accountAddress, currentAfterLt, beforeLt, limit);

            Transactions? response;
            try
            {
                response = await tonApiClient.GetBlockchainAccountTransactionsAsync(
                    accountAddress,
                    currentAfterLt,
                    beforeLt,
                    limit,
                    Sort_order.Asc,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Error fetching transactions for account {AccountAddress} after LT {AfterLt} before LT {BeforeLt}",
                    accountAddress, currentAfterLt, beforeLt);
                yield break;
            }

            if (response?.Transactions1 == null || response.Transactions1.Count == 0)
            {
                logger.LogDebug("No more transactions found for account {AccountAddress}", accountAddress);
                yield break;
            }

            foreach (Transaction? transaction in response.Transactions1) yield return transaction;

            if (response.Transactions1.Count < limit)
            {
                hasMore = false;
            }
            else
            {
                // Get LT of the last transaction for next page
                Transaction? lastTransaction = response.Transactions1.Last();
                currentAfterLt = lastTransaction.Lt;

                // Stop if we would go beyond beforeLt
                if (currentAfterLt >= beforeLt) hasMore = false;
            }
        }
    }

    public void Dispose()
    {
        rateLimiter.Dispose();
    }
}