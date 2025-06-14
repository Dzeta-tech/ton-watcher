using Dzeta.TonWatcher.Generated;
using DbTransaction = Dzeta.TonWatcher.Infrastructure.Entities.Transaction;

namespace Dzeta.TonWatcher.Core.Services;

public static class TransactionMapper
{
    public static DbTransaction ToDbTransaction(Transaction apiTransaction, string walletAddress)
    {
        return new DbTransaction
        {
            Hash = apiTransaction.Hash,
            Lt = apiTransaction.Lt,
            AccountAddress = walletAddress,
            Success = apiTransaction.Success,
            Utime = apiTransaction.Utime,
            WebhookNotified = false,
            TransactionData = apiTransaction
        };
    }

    public static void UpdateDbTransaction(DbTransaction dbTransaction, Transaction apiTransaction)
    {
        dbTransaction.Success = apiTransaction.Success;
        dbTransaction.Utime = apiTransaction.Utime;
        dbTransaction.TransactionData = apiTransaction;
    }
}