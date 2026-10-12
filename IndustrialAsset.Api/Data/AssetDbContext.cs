
using IndustrialAsset.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace IndustrialAsset.Api.Data;

public class AssetDbContext : DbContext
{
    public AssetDbContext(DbContextOptions<AssetDbContext> options)
        : base(options)
    {
    }

    public DbSet<Asset> Assets => Set<Asset>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Asset>()
            .HasIndex(asset => asset.AssetCode)
            .IsUnique();

        modelBuilder.Entity<Asset>()
            .Property(asset => asset.AssetCode)
            .HasMaxLength(20)
            .IsRequired();

        modelBuilder.Entity<Asset>()
            .Property(asset => asset.Equipment)
            .HasMaxLength(100)
            .IsRequired();

        modelBuilder.Entity<Asset>()
            .Property(asset => asset.Area)
            .HasMaxLength(100)
            .IsRequired();

        modelBuilder.Entity<Asset>()
            .Property(asset => asset.Status)
            .HasMaxLength(30)
            .IsRequired();
    }
}
