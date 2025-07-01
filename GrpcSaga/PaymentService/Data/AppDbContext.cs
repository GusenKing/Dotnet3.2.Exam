using Microsoft.EntityFrameworkCore;
using PaymentService.Data.Models;

namespace PaymentService.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Payment> Payments { get; set; }
}