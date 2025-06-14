namespace Dzeta.TonWatcher.Core;

public interface INotificationService
{
    Task SendPendingNotificationsAsync(CancellationToken cancellationToken = default);
}