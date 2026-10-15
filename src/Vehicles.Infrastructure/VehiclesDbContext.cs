using Microsoft.EntityFrameworkCore;
using Vehicles.Domain;

namespace Vehicles.Infrastructure;

public class VehiclesDbContext(DbContextOptions<VehiclesDbContext> options) : DbContext(options)
{
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Manufacturer> Manufacturers => Set<Manufacturer>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Manufacturer>(e =>
        {
            e.Property(m => m.Name).HasMaxLength(100).IsRequired();
            e.HasIndex(m => m.Name).IsUnique();
            e.HasData(
                new Manufacturer { Id = 1, Name = "Mazda" },
                new Manufacturer { Id = 2, Name = "Mercedes" },
                new Manufacturer { Id = 3, Name = "Honda" },
                new Manufacturer { Id = 4, Name = "Ferrari" },
                new Manufacturer { Id = 5, Name = "Toyota" });
        });

        modelBuilder.Entity<Category>(e =>
        {
            e.Property(c => c.Name).HasMaxLength(100).IsRequired();
            e.Property(c => c.Icon).HasMaxLength(50).IsRequired();
            e.Property(c => c.MinWeight).HasColumnType("decimal(10,2)");
            e.HasIndex(c => c.Name).IsUnique();
            e.HasIndex(c => c.MinWeight).IsUnique();
            e.ToTable(t => t.HasCheckConstraint("CK_Categories_MinWeight", "[MinWeight] >= 0"));
            e.HasData(
                new Category { Id = 1, Name = "Light", Icon = "feather", MinWeight = 0m },
                new Category { Id = 2, Name = "Medium", Icon = "car", MinWeight = 500m },
                new Category { Id = 3, Name = "Heavy", Icon = "truck", MinWeight = 2500m });
        });

        modelBuilder.Entity<Vehicle>(e =>
        {
            e.Property(v => v.OwnerName).HasMaxLength(200).IsRequired();
            e.Property(v => v.Weight).HasColumnType("decimal(10,2)");
            e.HasOne(v => v.Manufacturer)
                .WithMany()
                .HasForeignKey(v => v.ManufacturerId)
                .OnDelete(DeleteBehavior.Restrict);
            e.ToTable(t => t.HasCheckConstraint("CK_Vehicles_Weight", "[Weight] > 0"));
        });
    }
}
