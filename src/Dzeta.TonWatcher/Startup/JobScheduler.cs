using Dzeta.TonWatcher.Config;
using Dzeta.TonWatcher.Core;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace Dzeta.TonWatcher.Startup;

public static class JobScheduler
{
    public static void ScheduleJobs(TonWatcherConfiguration config, ILogger logger, IRecurringJobManager recurringJobManager)
    {
        ScheduleLatestTransactionsFetch(config, logger, recurringJobManager);
        ScheduleMissingTransactionsFix(logger, recurringJobManager);
        ScheduleWebhookNotifications(logger, recurringJobManager);

        logger.LogInformation("All recurring jobs have been scheduled");
    }

    static void ScheduleLatestTransactionsFetch(TonWatcherConfiguration config, ILogger logger, IRecurringJobManager recurringJobManager)
    {
        string? pollingCron = $"*/{config.PollingIntervalSeconds} * * * * *";

        recurringJobManager.AddOrUpdate<TransactionFetcherService>(
            "fetch-latest-transactions",
            service => service.FetchLatestTransactionsAsync(CancellationToken.None),
            pollingCron);

        logger.LogInformation("Scheduled 'fetch-latest-transactions' job with cron: {Cron}", pollingCron);
    }

    static void ScheduleMissingTransactionsFix(ILogger logger, IRecurringJobManager recurringJobManager)
    {
        recurringJobManager.AddOrUpdate<TransactionFetcherService>(
            "fix-missing-transactions",
            service => service.FixMissingTransactionsAsync(CancellationToken.None),
            "*/10 * * * *"); // Every 10 minutes

        logger.LogInformation("Scheduled 'fix-missing-transactions' job every 10 minutes");
    }

    static void ScheduleWebhookNotifications(ILogger logger, IRecurringJobManager recurringJobManager)
    {
        recurringJobManager.AddOrUpdate<INotificationService>(
            "send-webhook-notifications",
            service => service.SendPendingNotificationsAsync(CancellationToken.None),
            "*/10 * * * * *"); // Every 10 seconds

        logger.LogInformation("Scheduled 'send-webhook-notifications' job every 10 seconds");
    }
}