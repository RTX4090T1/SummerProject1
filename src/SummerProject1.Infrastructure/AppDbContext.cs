using Microsoft.EntityFrameworkCore;
using SummerProject1.Infrastructure.Entities;

namespace SummerProject1.Infrastructure;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Check> Checks => Set<Check>();
    public DbSet<CheckSource> CheckSources => Set<CheckSource>();
    public DbSet<SourceResult> SourceResults => Set<SourceResult>();
    public DbSet<MileageEvent> MileageEvents => Set<MileageEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Check>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Vin).HasMaxLength(17).IsRequired();
            b.Property(x => x.Mode).HasMaxLength(10).IsRequired();
            b.Property(x => x.Currency).HasMaxLength(3).IsRequired();
            b.Property(x => x.Status).HasMaxLength(20).IsRequired();
            b.Property(x => x.StripeCheckoutSessionId).HasMaxLength(200);
            b.Property(x => x.StripePaymentIntentId).HasMaxLength(200);
            b.HasMany(x => x.Sources).WithOne().HasForeignKey(x => x.CheckId);
        });

        modelBuilder.Entity<CheckSource>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.SourceId).HasMaxLength(40).IsRequired();
            b.Property(x => x.Status).HasMaxLength(20).IsRequired();
            b.HasIndex(x => new { x.CheckId, x.SourceId }).IsUnique();
        });

        modelBuilder.Entity<SourceResult>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.SourceId).HasMaxLength(40).IsRequired();
            b.Property(x => x.Provider).HasMaxLength(60).IsRequired();
            b.Property(x => x.Status).HasMaxLength(20).IsRequired();
            b.HasIndex(x => new { x.CheckId, x.SourceId, x.Provider }).IsUnique();
        });

        modelBuilder.Entity<MileageEvent>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.SourceId).HasMaxLength(40).IsRequired();
            b.Property(x => x.Provider).HasMaxLength(60).IsRequired();
            b.Property(x => x.Country).HasMaxLength(2);
            b.Property(x => x.Confidence).HasMaxLength(10).IsRequired();
            b.HasIndex(x => new { x.CheckId, x.SourceId });
        });
    }
}
