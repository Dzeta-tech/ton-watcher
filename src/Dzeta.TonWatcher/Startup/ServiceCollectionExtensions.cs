using Dzeta.Configuration;
using Dzeta.TonWatcher.Config;
using Dzeta.TonWatcher.Core;
using Dzeta.TonWatcher.Core.Services;
using Dzeta.TonWatcher.Generated;
using Dzeta.TonWatcher.Infrastructure;
using Dzeta.TonWatcher.Infrastructure.Repositories;
using Dzeta.TonWatcher.Providers;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Dzeta.TonWatcher.Startup;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTonWatcher(this IServiceCollection services)
    {
        // Configuration
        services.UseEnvironmentConfigurationProvider("TONWATCHER");
        services.AddConfiguration<TonWatcherConfiguration>();

        // Database
        services.AddDbContext<TonWatcherDbContext>((serviceProvider, options) =>
        {
            TonWatcherConfiguration? config = serviceProvider.GetRequiredService<TonWatcherConfiguration>();
            options.UseNpgsql(config.Database.ConnectionString);
        });

        // HTTP and TON API
        services.AddHttpClient();
        services.AddTonApiClient();

        // Application services
        services.AddSingleton<TonApiService>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<LatestTransactionFetcher>();
        services.AddScoped<MissingTransactionFetcher>();
        services.AddScoped<TransactionFetcherService>();
        services.AddScoped<INotificationService, WebhookNotificationService>();

        // Background jobs
        services.AddHangfireServices();

        return services;
    }

    static IServiceCollection AddTonApiClient(this IServiceCollection services)
    {
        services.AddSingleton<TonApiClient>(serviceProvider =>
        {
            TonWatcherConfiguration config = serviceProvider.GetRequiredService<TonWatcherConfiguration>();
            IHttpClientFactory httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            HttpClient httpClient = httpClientFactory.CreateClient();

            httpClient.BaseAddress = new Uri(config.TonApiUrl);
            httpClient.Timeout = TimeSpan.FromSeconds(config.HttpTimeoutSeconds);

            if (!string.IsNullOrEmpty(config.TonApiKey))
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.TonApiKey}");

            return new TonApiClient(httpClient);
        });

        return services;
    }

    static IServiceCollection AddHangfireServices(this IServiceCollection services)
    {
        services.AddHangfire(configuration => configuration
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseInMemoryStorage());

        services.AddHangfireServer(options => { options.WorkerCount = 1; });

        return services;
    }
}