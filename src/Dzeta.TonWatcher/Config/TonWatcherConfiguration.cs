using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Dzeta.Configuration;

namespace Dzeta.TonWatcher.Config;

/// <summary>
///     Конфигурация для TON Watcher микросервиса.
///     Все параметры задаются через ENV переменные с префиксом TONWATCHER_.
/// </summary>
public class TonWatcherConfiguration : BaseConfiguration
{
    /// <summary>
    ///     Адрес TON кошелька для отслеживания входящих транзакций.
    ///     ENV: TONWATCHER_WALLET_ADDRESS
    ///     Пример: "0:0fc009519c62c9262c9030bee05b5c805cb8f32ce3b8499791e0f82678075261"
    /// </summary>
    [Required]
    public string WalletAddress { get; set; } = string.Empty;

    /// <summary>
    ///     URL для отправки webhook'ов при получении новых транзакций.
    ///     ENV: TONWATCHER_WEBHOOK_URL
    ///     Пример: "https://api.mysite.com/webhooks/ton-payment"
    /// </summary>
    [Required]
    public string WebhookUrl { get; set; } = string.Empty;

    /// <summary>
    ///     Токен для авторизации webhook'ов (опционально).
    ///     Будет передаваться в заголовке Authorization: Bearer {token}
    ///     ENV: TONWATCHER_WEBHOOK_TOKEN
    /// </summary>
    public string? WebhookToken { get; set; }

    /// <summary>
    ///     Интервал проверки новых транзакций в секундах.
    ///     ENV: TONWATCHER_POLLING_INTERVAL_SECONDS
    ///     По умолчанию: 30 секунд
    /// </summary>
    [DefaultValue(30)]
    public int PollingIntervalSeconds { get; set; }

    /// <summary>
    ///     URL TON API провайдера.
    ///     ENV: TONWATCHER_TON_API_URL
    ///     По умолчанию: https://tonapi.io
    /// </summary>
    [DefaultValue("https://tonapi.io")]
    public string TonApiUrl { get; set; } = "https://tonapi.io";

    /// <summary>
    ///     API ключ для TON API (если требуется).
    ///     ENV: TONWATCHER_TON_API_KEY
    /// </summary>
    public string? TonApiKey { get; set; }

    /// <summary>
    ///     Максимальное количество попыток отправки webhook'а.
    ///     ENV: TONWATCHER_WEBHOOK_MAX_RETRIES
    ///     По умолчанию: 3
    /// </summary>
    [DefaultValue(3)]
    public int WebhookMaxRetries { get; set; }

    /// <summary>
    ///     Таймаут HTTP запросов в секундах.
    ///     ENV: TONWATCHER_HTTP_TIMEOUT_SECONDS
    ///     По умолчанию: 30 секунд
    /// </summary>
    [DefaultValue(30)]
    public int HttpTimeoutSeconds { get; set; }

    /// <summary>
    ///     Конфигурация подключения к базе данных PostgreSQL.
    ///     ENV переменные: TONWATCHER_DATABASE_*
    /// </summary>
    [Required]
    public DatabaseConnectionConfiguration Database { get; set; } = new();
}