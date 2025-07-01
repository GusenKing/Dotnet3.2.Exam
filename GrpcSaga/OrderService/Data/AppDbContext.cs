using Microsoft.EntityFrameworkCore;
using OrderService.Data.Models;

namespace OrderService.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().HasData(
            new Product { ProductName = "Brick", ProductQuantity = 1000, ProductPrice = 1 },
            new Product { ProductName = "Custom PC", ProductQuantity = 1, ProductPrice = 10000 });
    }
}