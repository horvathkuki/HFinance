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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Holding>()
            .Property(h => h.Quantity)
            .HasColumnType("decimal(18,4)");

        modelBuilder.Entity<Holding>()
            .Property(h => h.AveragePurchasePrice)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Portfolio>()
            .HasIndex(p => p.UserId);
    }
}
