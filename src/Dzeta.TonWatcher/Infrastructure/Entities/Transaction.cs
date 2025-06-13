using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Dzeta.TonWatcher.Infrastructure.Entities;

[Table("transactions")]
[Index(nameof(Lt))]
[Index(nameof(WebhookNotified))]
public class Transaction
{
    [Key] [Column("hash")] [MaxLength(64)] public string Hash { get; set; } = string.Empty;

    [Column("lt")] public long Lt { get; set; }

    [Column("account_address")]
    [MaxLength(70)]
    public string AccountAddress { get; set; } = string.Empty;

    [Column("success")] public bool Success { get; set; }

    [Column("utime")] public long Utime { get; set; }

    [Column("webhook_notified")] public bool WebhookNotified { get; set; }

    [Column("webhook_notified_at")] public DateTime? WebhookNotifiedAt { get; set; }

    [Column("transaction_data", TypeName = "jsonb")]
    public string TransactionDataJson { get; set; } = string.Empty;

    [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     Gets the deserialized transaction data from the JSONB column
    /// </summary>
    [NotMapped]
    public Generated.Transaction? TransactionData
    {
        get
        {
            if (string.IsNullOrEmpty(TransactionDataJson))
                return null;

            try
            {
                return JsonSerializer.Deserialize<Generated.Transaction>(TransactionDataJson);
            }
            catch (JsonException)
            {
                return null;
            }
        }
        set =>
            TransactionDataJson = value != null
                ? JsonSerializer.Serialize(value)
                : string.Empty;
    }
}