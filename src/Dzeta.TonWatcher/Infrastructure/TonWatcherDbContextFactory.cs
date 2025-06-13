using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Dzeta.TonWatcher.Infrastructure;

/// <summary>
///     Design-time factory for creating DbContext during migrations
/// </summary>
public class TonWatcherDbContextFactory : IDesignTimeDbContextFactory<TonWatcherDbContext>
{
    public TonWatcherDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<TonWatcherDbContext> optionsBuilder = new();

        // Use a dummy connection string for design-time operations
        // The actual connection string will be provided at runtime
        optionsBuilder.UseNpgsql("Host=localhost;Database=tonwatcher_designtime;Username=postgres;");

        return new TonWatcherDbContext(optionsBuilder.Options);
    }
}