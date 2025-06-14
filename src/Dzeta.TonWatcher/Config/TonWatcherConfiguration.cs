using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Dzeta.Configuration;

namespace Dzeta.TonWatcher.Config;

/// <summary>
///     Configuration for TON Watcher microservice.
///     All parameters are set via ENV variables with TONWATCHER_ prefix.
/// </summary>
public class TonWatcherConfiguration : BaseConfiguration
{
    /// <summary>
    ///     TON wallet address to track incoming transactions.
    ///     ENV: TONWATCHER_WALLET_ADDRESS
    ///     Example: "0:0fc009519c62c9262c9030bee05b5c805cb8f32ce3b8499791e0f82678075261"
    /// </summary>
    [Required]
    public string WalletAddress { get; set; } = string.Empty;

    /// <summary>
    ///     URL for sending webhooks when new transactions are received.
    ///     ENV: TONWATCHER_WEBHOOK_URL
    ///     Example: "https://api.mysite.com/webhooks/ton-payment"
    /// </summary>
    [Required]
    public string WebhookUrl { get; set; } = string.Empty;

    /// <summary>
    ///     Token for webhook authorization (optional).
    ///     Will be passed in Authorization: Bearer {token} header
    ///     ENV: TONWATCHER_WEBHOOK_TOKEN
    /// </summary>
    public string? WebhookToken { get; set; }

    /// <summary>
    ///     Interval for checking new transactions in seconds.
    ///     ENV: TONWATCHER_POLLING_INTERVAL_SECONDS
    ///     Default: 30 seconds
    /// </summary>
    [DefaultValue(30)]
    public int PollingIntervalSeconds { get; set; }

    /// <summary>
    ///     TON API provider URL.
    ///     ENV: TONWATCHER_TON_API_URL
    ///     Default: https://tonapi.io
    /// </summary>
    [DefaultValue("https://tonapi.io")]
    public string TonApiUrl { get; set; } = "https://tonapi.io";

    /// <summary>
    ///     API key for TON API (if required).
    ///     ENV: TONWATCHER_TON_API_KEY
    /// </summary>
    public string? TonApiKey { get; set; }

    /// <summary>
    ///     Maximum number of webhook delivery attempts.
    ///     ENV: TONWATCHER_WEBHOOK_MAX_RETRIES
    ///     Default: 3
    /// </summary>
    [DefaultValue(3)]
    public int WebhookMaxRetries { get; set; }

    /// <summary>
    ///     HTTP request timeout in seconds.
    ///     ENV: TONWATCHER_HTTP_TIMEOUT_SECONDS
    ///     Default: 30 seconds
    /// </summary>
    [DefaultValue(30)]
    public int HttpTimeoutSeconds { get; set; }

    /// <summary>
    ///     PostgreSQL database connection configuration.
    ///     ENV variables: TONWATCHER_DATABASE_*
    /// </summary>
    [Required]
    public DatabaseConnectionConfiguration Database { get; set; } = new();
}