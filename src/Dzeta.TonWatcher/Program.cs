using Dzeta.TonWatcher.Config;
using Dzeta.TonWatcher.Startup;
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
            var builder = Host.CreateApplicationBuilder(args);
            
            builder.Services.AddTonWatcher();
            builder.Services.AddSerilog();

            var host = builder.Build();
            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            var config = host.Services.GetRequiredService<TonWatcherConfiguration>();

            LogStartupInfo(logger, config);
            JobScheduler.ScheduleJobs(config, logger);

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

    private static void LogStartupInfo(ILogger<Program> logger, TonWatcherConfiguration config)
    {
        logger.LogInformation("Starting Dzeta.TonWatcher...");
        logger.LogInformation("Watching wallet: {WalletAddress}", config.WalletAddress);
        logger.LogInformation("Webhook URL: {WebhookUrl}", config.WebhookUrl);
        logger.LogInformation("Polling interval: {Interval}s", config.PollingIntervalSeconds);
        logger.LogInformation("TON API: {ApiUrl}", config.TonApiUrl);
    }
}