using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Dzeta.TonWatcher.Config;
using Dzeta.TonWatcher.Core.Models;
using Dzeta.TonWatcher.Infrastructure;
using Dzeta.TonWatcher.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dzeta.TonWatcher.Core;

public class WebhookNotificationService(
    TonWatcherDbContext dbContext,
    HttpClient httpClient,
    TonWatcherConfiguration config,
    ILogger<WebhookNotificationService> logger) : INotificationService
{
    public async Task SendPendingNotificationsAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting webhook notification process");

        List<Transaction>? pendingTransactions = await dbContext.Transactions
            .Where(t => !t.WebhookNotified)
            .OrderBy(t => t.Utime)
            .ToListAsync(cancellationToken);

        if (pendingTransactions.Count == 0)
        {
            logger.LogDebug("No pending transactions to notify");
            return;
        }

        logger.LogInformation("Found {Count} transactions to notify", pendingTransactions.Count);

        foreach (Transaction? transaction in pendingTransactions)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            await SendWebhookWithRetryAsync(transaction, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Webhook notification process completed");
    }

    async Task SendWebhookWithRetryAsync(Transaction transaction, CancellationToken cancellationToken)
    {
        int maxRetries = config.WebhookMaxRetries;
        int attempt = 0;

        while (attempt < maxRetries && !cancellationToken.IsCancellationRequested)
        {
            attempt++;

            try
            {
                logger.LogDebug("Sending webhook for transaction {Hash}, attempt {Attempt}/{MaxRetries}",
                    transaction.Hash, attempt, maxRetries);

                bool success = await SendWebhookAsync(transaction, cancellationToken);

                if (success)
                {
                    transaction.WebhookNotified = true;
                    transaction.WebhookNotifiedAt = DateTime.UtcNow;

                    logger.LogInformation("Successfully sent webhook for transaction {Hash}", transaction.Hash);
                    return;
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to send webhook for transaction {Hash}, attempt {Attempt}/{MaxRetries}",
                    transaction.Hash, attempt, maxRetries);
            }

            if (attempt < maxRetries)
            {
                TimeSpan delay = TimeSpan.FromSeconds(Math.Pow(2, attempt)); // Exponential backoff
                logger.LogDebug("Waiting {Delay} seconds before retry", delay.TotalSeconds);
                await Task.Delay(delay, cancellationToken);
            }
        }

        logger.LogError("Failed to send webhook for transaction {Hash} after {MaxRetries} attempts",
            transaction.Hash, maxRetries);
    }

    async Task<bool> SendWebhookAsync(Transaction transaction, CancellationToken cancellationToken)
    {
        using HttpRequestMessage request = new(HttpMethod.Post, config.WebhookUrl);

        // Set authorization header if token is provided
        if (!string.IsNullOrEmpty(config.WebhookToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", config.WebhookToken);

        // Create webhook payload
        WebhookPayload payload = new()
        {
            Hash = transaction.Hash,
            Lt = transaction.Lt,
            AccountAddress = transaction.AccountAddress,
            Success = transaction.Success,
            Utime = transaction.Utime,
            CreatedAt = transaction.CreatedAt,
            TransactionData = transaction.TransactionData
        };

        string jsonContent = JsonSerializer.Serialize(payload);

        request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);

        logger.LogDebug("Webhook response for transaction {Hash}: {StatusCode}",
            transaction.Hash, response.StatusCode);

        return response.IsSuccessStatusCode;
    }
}