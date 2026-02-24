using backend.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Portfolio> Portfolios { get; set; }
    public DbSet<Holding> Holdings { get; set; }
    public DbSet<HoldingGroup> HoldingGroups { get; set; }
    public DbSet<PortfolioSnapshot> PortfolioSnapshots { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Holding>()
            .Property(h => h.Quantity)
            .HasColumnType("decimal(18,4)");

        modelBuilder.Entity<Holding>()
            .Property(h => h.AveragePurchasePrice)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Holding>()
            .HasOne(h => h.Group)
            .WithMany(g => g.Holdings)
            .HasForeignKey(h => h.GroupId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Holding>()
            .ToTable(t => t.HasCheckConstraint("CK_Holdings_Currency", "[Currency] IN ('EUR', 'USD', 'RON')"));

        modelBuilder.Entity<Portfolio>()
            .HasIndex(p => p.UserId);

        modelBuilder.Entity<HoldingGroup>()
            .HasIndex(g => new { g.UserId, g.Name })
            .IsUnique();

        modelBuilder.Entity<PortfolioSnapshot>()
            .HasIndex(s => new { s.PortfolioId, s.CapturedAtUtc });

        modelBuilder.Entity<PortfolioSnapshot>()
            .Property(snapshot => snapshot.TotalMarketValueBase)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<PortfolioSnapshot>()
            .Property(snapshot => snapshot.TotalCostBasisBase)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<PortfolioSnapshot>()
            .Property(snapshot => snapshot.TotalUnrealizedPnLBase)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<PortfolioSnapshot>()
            .Property(snapshot => snapshot.EurUsdRate)
            .HasColumnType("decimal(18,6)");

        modelBuilder.Entity<PortfolioSnapshot>()
            .Property(snapshot => snapshot.EurRonRate)
            .HasColumnType("decimal(18,6)");

        modelBuilder.Entity<PortfolioSnapshot>()
            .HasOne(s => s.Portfolio)
            .WithMany(p => p.Snapshots)
            .HasForeignKey(s => s.PortfolioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
