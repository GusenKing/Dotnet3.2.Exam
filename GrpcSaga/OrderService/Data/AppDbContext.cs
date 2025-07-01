using Microsoft.EntityFrameworkCore;
using OrderService.Data.Models;

namespace OrderService.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<Product> Products { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSeeding((context, _) =>
            {
                context.Set<Product>().AddRange(
                    new Product { ProductName = "Brick", ProductQuantity = 1000, ProductPrice = 1 },
                    new Product { ProductName = "Custom PC", ProductQuantity = 1, ProductPrice = 10000 });
                context.SaveChanges();
            })
            .UseAsyncSeeding(async (context, _, ctx) =>
            {
                await context.Set<Product>().AddRangeAsync([
                    new Product { ProductName = "Brick", ProductQuantity = 1000, ProductPrice = 1 },
                    new Product { ProductName = "Custom PC", ProductQuantity = 1, ProductPrice = 10000 }
                ], ctx);
                await context.SaveChangesAsync(ctx);
            });
    }
}