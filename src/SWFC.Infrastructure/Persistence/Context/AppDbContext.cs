using Microsoft.EntityFrameworkCore;

namespace SWFC.Infrastructure;

public sealed class AppDbContext : DbContext
{
    public const string DefaultSchema = "core";

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(DefaultSchema);
        base.OnModelCreating(modelBuilder);
    }
}
