using System.Text.Json.Serialization;
using Dzeta.TonWatcher.Generated;

namespace Dzeta.TonWatcher.Core.Models;

public class WebhookPayload
{
    [JsonPropertyName("hash")] public string Hash { get; set; } = string.Empty;

    [JsonPropertyName("lt")] public long Lt { get; set; }

    [JsonPropertyName("account_address")] public string AccountAddress { get; set; } = string.Empty;

    [JsonPropertyName("success")] public bool Success { get; set; }

    [JsonPropertyName("utime")] public long Utime { get; set; }

    [JsonPropertyName("created_at")] public DateTime CreatedAt { get; set; }

    [JsonPropertyName("transaction_data")] public Transaction? TransactionData { get; set; }
}