using Dzeta.TonWatcher.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dzeta.TonWatcher.Infrastructure;

public class TonWatcherDbContext(DbContextOptions<TonWatcherDbContext> options) : DbContext(options)
{
    public DbSet<Transaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure PostgreSQL-specific features
        modelBuilder.HasPostgresExtension("uuid-ossp");
    }
}