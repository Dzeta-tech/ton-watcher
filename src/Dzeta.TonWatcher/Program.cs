using Dzeta.TonWatcher.Config;
using Dzeta.TonWatcher.Infrastructure;
using Dzeta.TonWatcher.Startup;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Dzeta.TonWatcher;

internal class Program
{
    static async Task Main(string[] args)
    {
        LoggingConfiguration.ConfigureSerilog();

        try
        {
            HostApplicationBuilder? builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddTonWatcher();
            builder.Services.AddSerilog();

            IHost? host = builder.Build();
            ILogger<Program>? logger = host.Services.GetRequiredService<ILogger<Program>>();
            TonWatcherConfiguration? config = host.Services.GetRequiredService<TonWatcherConfiguration>();

            LogStartupInfo(logger, config);
            
            // Ensure database is created
            await EnsureDatabaseCreated(host, logger);
            
            // Schedule jobs AFTER host is built so Hangfire services are initialized
            IRecurringJobManager recurringJobManager = host.Services.GetRequiredService<IRecurringJobManager>();
            JobScheduler.ScheduleJobs(config, logger, recurringJobManager);

            await host.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
            throw;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    static async Task EnsureDatabaseCreated(IHost host, Microsoft.Extensions.Logging.ILogger logger)
    {
        using IServiceScope scope = host.Services.CreateScope();
        TonWatcherDbContext dbContext = scope.ServiceProvider.GetRequiredService<TonWatcherDbContext>();
        
        logger.LogInformation("Ensuring database is created...");
        await dbContext.Database.EnsureCreatedAsync();
        logger.LogInformation("Database is ready");
    }

    static void LogStartupInfo(ILogger<Program> logger, TonWatcherConfiguration config)
    {
        logger.LogInformation("Starting Dzeta.TonWatcher...");
        logger.LogInformation("Watching wallet: {WalletAddress}", config.WalletAddress);
        logger.LogInformation("Webhook URL: {WebhookUrl}", config.WebhookUrl);
        logger.LogInformation("Polling interval: {Interval}s", config.PollingIntervalSeconds);
        logger.LogInformation("TON API: {ApiUrl}", config.TonApiUrl);
    }
}