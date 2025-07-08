# TON Watcher Examples

This directory contains examples and testing tools for the TON Watcher project.

## 🧪 Testing Environment

The example setup provides a complete testing environment with:

- **PostgreSQL Database** - Persistent storage for transactions
- **TON Watcher Service** - Main application monitoring the blockchain
- **Webhook Listener** - Testing service to receive and display webhooks

## 🚀 Quick Start

### 1. Start the Testing Environment

```bash
# Start all services
docker-compose up --build

# Start in background
docker-compose up -d --build
```

### 2. Monitor Webhook Activity

```bash
# Follow webhook listener logs to see incoming transactions
docker-compose logs -f webhook-listener

# Follow TON Watcher logs
docker-compose logs -f ton-watcher
```

### 3. Stop the Environment

```bash
# Stop all services
docker-compose down

# Stop and remove volumes (clean slate)
docker-compose down -v
```

## 📋 Services Overview

### PostgreSQL Database
- **Port**: 5432
- **Database**: `tonwatcher`
- **Username**: `tonwatcher`
- **Password**: `tonwatcher123`
- **Persistent Storage**: Data survives container restarts

### TON Watcher
- **Monitoring Wallet**: `0:6f3aab06db6b19504c9c0eaee61346ad860fd476b529c041ece7416cc9b15b57`
- **Polling Interval**: 30 seconds
- **TON API**: https://tonapi.io
- **Webhook Target**: Internal webhook-listener service

### Webhook Listener
- **Port**: 8080
- **Endpoint**: `/webhook`
- **Authentication**: Bearer token (`test-token-123`)
- **Purpose**: Displays received webhooks in console

## 🔧 Configuration

The example environment uses these settings:

```yaml
# Database Configuration
TONWATCHER_DATABASE_HOST: postgres
TONWATCHER_DATABASE_PORT: 5432
TONWATCHER_DATABASE_DATABASE: tonwatcher
TONWATCHER_DATABASE_USERNAME: tonwatcher
TONWATCHER_DATABASE_PASSWORD: tonwatcher123

# Monitoring Configuration
TONWATCHER_WALLET_ADDRESS: 0:6f3aab06db6b19504c9c0eaee61346ad860fd476b529c041ece7416cc9b15b57
TONWATCHER_POLLING_INTERVAL_SECONDS: 30

# Webhook Configuration
TONWATCHER_WEBHOOK_URL: http://webhook-listener:8080/webhook
TONWATCHER_WEBHOOK_TOKEN: test-token-123
TONWATCHER_WEBHOOK_MAX_RETRIES: 3

# TON API Configuration
TONWATCHER_TON_API_URL: https://tonapi.io
TONWATCHER_HTTP_TIMEOUT_SECONDS: 30
```

## 📊 What to Expect

### Successful Startup Logs
```
ton-watcher-1       | [16:44:04 INF] Starting Dzeta.TonWatcher...
ton-watcher-1       | [16:44:04 INF] Watching wallet: 0:6f3aab06db6b19504c9c0eaee61346ad860fd476b529c041ece7416cc9b15b57
ton-watcher-1       | [16:44:04 INF] Webhook URL: http://webhook-listener:8080/webhook
ton-watcher-1       | [16:44:04 INF] Database is ready
ton-watcher-1       | [16:44:04 INF] All recurring jobs have been scheduled
```

### Transaction Webhook Example
```
=== WEBHOOK RECEIVED ===
Timestamp: 2025-07-07 16:44:50 UTC
Authorization: Bearer test-token-123

{
  "hash": "27a9abceec56f205456ff793472f664848e4f7badcc1f4be82ddf81affacf88f",
  "lt": 58740555000004,
  "account_address": "0:6f3aab06db6b19504c9c0eaee61346ad860fd476b529c041ece7416cc9b15b57",
  "success": true,
  "utime": 1750927130,
  "transaction_data": {
    "total_fees": 396405,
    "end_balance": 604899160,
    "in_msg": {
      "value": 90949587,
      "decoded_op_name": "excess"
    }
  }
}
```

## 🛠️ Customization

### Monitor Different Wallet
Edit `docker-compose.yml` and change:
```yaml
- TONWATCHER_WALLET_ADDRESS=your_wallet_address_here
```

### Change Polling Frequency
```yaml
- TONWATCHER_POLLING_INTERVAL_SECONDS=10  # Poll every 10 seconds
```

### Use Your Webhook Endpoint
```yaml
- TONWATCHER_WEBHOOK_URL=https://your-domain.com/webhook
- TONWATCHER_WEBHOOK_TOKEN=your_secret_token
```

## 🧩 Webhook Listener

The included webhook listener is a simple .NET 8 minimal API that:

- Accepts POST requests to `/webhook`
- Validates bearer token authentication
- Pretty-prints received JSON payloads
- Returns success responses to TON Watcher

### Source Code
The webhook listener source is in `webhook-listener/Program.cs` and can be customized for your needs.

## 🔍 Troubleshooting

### Common Issues

**Database Connection Failed**
```bash
# Check if PostgreSQL is ready
docker-compose logs postgres

# Wait for health check to pass
docker-compose ps
```

**No Transactions Appearing**
- Verify the wallet address has recent activity
- Check TON API connectivity: `curl https://tonapi.io`
- Ensure polling interval allows time for transactions

**Webhook Delivery Failed**
```bash
# Check webhook listener logs
docker-compose logs webhook-listener

# Verify internal networking
docker-compose exec ton-watcher ping webhook-listener
```

### Accessing Database
```bash
# Connect to PostgreSQL
docker-compose exec postgres psql -U tonwatcher -d tonwatcher

# View transactions
SELECT hash, lt, success, utime FROM transactions ORDER BY lt DESC LIMIT 10;
```

### Rebuilding Services
```bash
# Rebuild specific service
docker-compose build ton-watcher

# Rebuild and restart
docker-compose up --build --force-recreate ton-watcher
```

## 📈 Performance Testing

### Generate Load
To test with higher transaction volumes:

1. Monitor a busy wallet address
2. Reduce polling interval to 5-10 seconds
3. Watch batch processing in action

### Monitor Resource Usage
```bash
# Container resource usage
docker stats

# Database queries
docker-compose exec postgres psql -U tonwatcher -d tonwatcher -c "
SELECT schemaname,tablename,attname,n_distinct,correlation 
FROM pg_stats WHERE tablename = 'transactions';
"
```

---

This example environment provides everything needed to test and understand TON Watcher's capabilities in a controlled environment. 