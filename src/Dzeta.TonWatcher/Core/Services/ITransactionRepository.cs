using Dzeta.TonWatcher.Infrastructure.Entities;

namespace Dzeta.TonWatcher.Core.Services;

public interface ITransactionRepository
{
    Task<long> GetMaxLtAsync(string walletAddress, CancellationToken cancellationToken = default);
    Task<Transaction?> GetByHashAsync(string hash, CancellationToken cancellationToken = default);
    Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default);
    Task UpdateAsync(Transaction transaction, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
} 