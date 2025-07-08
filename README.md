# TON Watcher

A microservice for monitoring TON blockchain transactions with real-time webhook notifications and comprehensive transaction tracking.

![TON Watcher](https://img.shields.io/badge/TON-Blockchain-blue)
![.NET 8](https://img.shields.io/badge/.NET-8.0-purple)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15-blue)
![Docker](https://img.shields.io/badge/Docker-Ready-green)

## 🎯 Overview

TON Watcher is a transaction monitoring service designed to track incoming and outgoing transactions for TON blockchain wallets. It provides webhook notifications, automatic gap filling, and comprehensive transaction data storage.

## ✨ Features

### Core Functionality
- **Real-time Transaction Monitoring** - Continuous polling of TON blockchain via TonAPI
- **Webhook Notifications** - Reliable delivery with exponential backoff retry mechanism
- **Gap Detection & Recovery** - Automatic detection and filling of missing transactions
- **Comprehensive Data Storage** - Full transaction metadata stored in PostgreSQL with JSONB support
- **Rate Limiting** - Built-in rate limiting to respect API constraints

### Technical Features
- **Background Job Processing** - Powered by Hangfire for reliable scheduling
- **Structured Logging** - Comprehensive logging with Serilog
- **Docker Support** - Production-ready containerization
- **Health Monitoring** - Built-in health checks and monitoring
- **Configuration Management** - Environment-based configuration with validation
- **Database Migrations** - Automatic database schema management

## 🚀 Quick Start

### Prerequisites
- .NET 8.0 SDK
- PostgreSQL 15+
- Docker (optional)

### Using Docker (Recommended)

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd ton-watcher
   ```

2. **Start with Docker Compose**
   ```bash
   cd examples
   docker-compose up --build
   ```

This will start:
- PostgreSQL database
- TON Watcher service
- Webhook listener (for testing)

### Manual Installation

1. **Install dependencies**
   ```bash
   dotnet restore src/Dzeta.TonWatcher/Dzeta.TonWatcher.csproj
   ```

2. **Configure environment variables** (see [Configuration](#configuration))

3. **Run the application**
   ```bash
   cd src/Dzeta.TonWatcher
   dotnet run
   ```

## ⚙️ Configuration

TON Watcher is configured via environment variables with the `TONWATCHER_` prefix:

### Required Configuration

| Variable | Description | Example |
|----------|-------------|---------|
| `TONWATCHER_WALLET_ADDRESS` | TON wallet address to monitor | `0:6f3aab06db6b19504c9c0eaee61346ad860fd476b529c041ece7416cc9b15b57` |
| `TONWATCHER_WEBHOOK_URL` | Webhook endpoint URL | `https://api.example.com/webhooks/ton-payment` |
| `TONWATCHER_DATABASE_HOST` | PostgreSQL host | `localhost` |
| `TONWATCHER_DATABASE_PORT` | PostgreSQL port | `5432` |
| `TONWATCHER_DATABASE_DATABASE` | Database name | `tonwatcher` |
| `TONWATCHER_DATABASE_USERNAME` | Database username | `tonwatcher` |
| `TONWATCHER_DATABASE_PASSWORD` | Database password | `secure_password` |

### Optional Configuration

| Variable | Default | Description |
|----------|---------|-------------|
| `TONWATCHER_WEBHOOK_TOKEN` | - | Bearer token for webhook authentication |
| `TONWATCHER_POLLING_INTERVAL_SECONDS` | `30` | Transaction polling interval |
| `TONWATCHER_TON_API_URL` | `https://tonapi.io` | TON API provider URL |
| `TONWATCHER_TON_API_KEY` | - | API key for TON API (if required) |
| `TONWATCHER_WEBHOOK_MAX_RETRIES` | `3` | Maximum webhook delivery attempts |
| `TONWATCHER_HTTP_TIMEOUT_SECONDS` | `30` | HTTP request timeout |

## 📡 Webhook Payload

When a transaction is detected, TON Watcher sends a POST request to your webhook URL:

```json
{
  "hash": "27a9abceec56f205456ff793472f664848e4f7badcc1f4be82ddf81affacf88f",
  "lt": 58740555000004,
  "account_address": "0:6f3aab06db6b19504c9c0eaee61346ad860fd476b529c041ece7416cc9b15b57",
  "success": true,
  "utime": 1750927130,
  "created_at": "2025-07-07T16:44:35.731754Z",
  "transaction_data": {
    "hash": "27a9abceec56f205456ff793472f664848e4f7badcc1f4be82ddf81affacf88f",
    "lt": 58740555000004,
    "account": {
      "address": "0:6f3aab06db6b19504c9c0eaee61346ad860fd476b529c041ece7416cc9b15b57",
      "is_wallet": true
    },
    "success": true,
    "utime": 1750927130,
    "total_fees": 396405,
    "end_balance": 604899160,
    "in_msg": {
      "value": 90949587,
      "decoded_op_name": "excess"
    }
  }
}
```

## 🏗️ Architecture

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│  TON Blockchain │    │   TON Watcher    │    │  Your Webhook   │
│                 │───▶│                  │───▶│    Endpoint     │
│     TonAPI      │    │  - Fetcher       │    │                 │
└─────────────────┘    │  - Processor     │    └─────────────────┘
                       │  - Notifier      │
                       └─────────┬────────┘
                                 │
                       ┌─────────▼────────┐
                       │   PostgreSQL     │
                       │   Database       │
                       └──────────────────┘
```

### Core Components

- **Transaction Fetcher** - Polls TON API for new transactions
- **Batch Processor** - Optimizes database operations
- **Webhook Notifier** - Delivers notifications with retry logic
- **Missing Transaction Recovery** - Fills gaps in transaction history
- **Background Scheduler** - Manages recurring jobs with Hangfire

## 🔧 Development

### Project Structure
```
src/Dzeta.TonWatcher/
├── Core/                 # Business logic and services
├── Infrastructure/       # Data access and external services
├── Config/              # Configuration models
├── Providers/           # External service providers
├── Startup/             # Application startup and DI
└── Generated/           # Auto-generated API clients
```

## 📊 Monitoring & Observability

TON Watcher provides comprehensive logging and monitoring:

- **Structured Logging** - JSON-formatted logs with correlation IDs
- **Performance Metrics** - Transaction processing times and rates
- **Health Checks** - Database connectivity and API availability
- **Error Tracking** - Detailed error reporting with stack traces

### Log Levels
- `Information` - Normal operation events
- `Warning` - Recoverable errors (API rate limits, temporary failures)
- `Error` - Processing errors requiring attention
- `Fatal` - Application-level failures

## 🛣️ Roadmap

- [ ] **Custom Rate Limiting** - Configurable rate limits per API provider
- [ ] **API Provider Abstraction** - Support for multiple blockchain data providers
- [ ] **Advanced Pagination** - Custom page sizes and offset controls

## 🤝 Contributing

1. **Fork the repository**
2. **Create a feature branch** (`git checkout -b feature/amazing-feature`)
3. **Commit your changes** (`git commit -m 'Add amazing feature'`)
4. **Push to the branch** (`git push origin feature/amazing-feature`)
5. **Open a Pull Request**

### Development Guidelines
- Follow C# coding conventions and .NET best practices
- Write unit tests for new functionality
- Update documentation for API changes
- Ensure Docker builds work correctly

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

- **Issues** - Report bugs via [GitHub Issues](../../issues)
