using Dzeta.TonWatcher.Config;
using Dzeta.TonWatcher.Core;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace Dzeta.TonWatcher.Startup;

public static class JobScheduler
{
    public static void ScheduleJobs(TonWatcherConfiguration config, ILogger logger)
    {
        ScheduleLatestTransactionsFetch(config, logger);
        ScheduleMissingTransactionsFix(logger);

        logger.LogInformation("All recurring jobs have been scheduled");
    }

    static void ScheduleLatestTransactionsFetch(TonWatcherConfiguration config, ILogger logger)
    {
        string? pollingCron = $"*/{config.PollingIntervalSeconds} * * * * *";

        RecurringJob.AddOrUpdate<TransactionFetcherService>(
            "fetch-latest-transactions",
            service => service.FetchLatestTransactionsAsync(CancellationToken.None),
            pollingCron);

        logger.LogInformation("Scheduled 'fetch-latest-transactions' job with cron: {Cron}", pollingCron);
    }

    static void ScheduleMissingTransactionsFix(ILogger logger)
    {
        RecurringJob.AddOrUpdate<TransactionFetcherService>(
            "fix-missing-transactions",
            service => service.FixMissingTransactionsAsync(CancellationToken.None),
            "*/10 * * * *"); // Every 10 minutes

        logger.LogInformation("Scheduled 'fix-missing-transactions' job every 10 minutes");
    }
}