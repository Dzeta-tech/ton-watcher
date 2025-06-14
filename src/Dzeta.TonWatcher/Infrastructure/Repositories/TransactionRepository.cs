using Dzeta.TonWatcher.Core.Services;
using Dzeta.TonWatcher.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dzeta.TonWatcher.Infrastructure.Repositories;

public class TransactionRepository(TonWatcherDbContext dbContext) : ITransactionRepository
{
    public async Task<long> GetMaxLtAsync(string walletAddress, CancellationToken cancellationToken = default)
    {
        return await dbContext.Transactions
            .Where(t => t.AccountAddress == walletAddress)
            .MaxAsync(t => (long?)t.Lt, cancellationToken) ?? 0;
    }

    public async Task<Transaction?> GetByHashAsync(string hash, CancellationToken cancellationToken = default)
    {
        return await dbContext.Transactions
            .FirstOrDefaultAsync(t => t.Hash == hash, cancellationToken);
    }

    public Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        dbContext.Transactions.Add(transaction);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        dbContext.Transactions.Update(transaction);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}